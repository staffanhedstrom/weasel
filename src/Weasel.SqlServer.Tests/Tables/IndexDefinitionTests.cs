using System.Collections.Generic;
using Shouldly;
using Weasel.SqlServer.Tables;
using Xunit;

namespace Weasel.SqlServer.Tests.Tables
{
    public class IndexDefinitionTests
    {
        private IndexDefinition theIndex = new IndexDefinition("idx_1")
            .AgainstColumns("column1");
        
        private Table parent = new Table("people");
        

        [Fact]
        public void default_sort_order_is_asc()
        {
            theIndex.SortOrder.ShouldBe(SortOrder.Asc);
        }


        [Fact]
        public void is_not_unique_by_default()
        {
            theIndex.IsUnique.ShouldBeFalse();
        }



        [Fact]
        public void write_basic_index()
        {
            theIndex.ToDDL(parent)
                .ShouldBe("CREATE INDEX idx_1 ON public.people USING btree (column1);");
        }
        
        [Fact]
        public void write_unique_index()
        {
            theIndex.IsUnique = true;
            
            theIndex.ToDDL(parent)
                .ShouldBe("CREATE UNIQUE INDEX idx_1 ON public.people USING btree (column1);");
        }

        [Fact]
        public void write_desc()
        {
            theIndex.SortOrder = SortOrder.Desc;
            
            theIndex.ToDDL(parent)
                .ShouldBe("CREATE INDEX idx_1 ON public.people USING btree (column1 DESC);");
        }
        

        [Fact]
        public void with_a_predicate()
        {
            theIndex.Predicate = "foo > 1";
            
            theIndex.ToDDL(parent)
                .ShouldBe("CREATE INDEX idx_1 ON public.people WHERE (foo > 1);");
        }

        [Fact]
        public void with_a_non_default_fill_factor()
        {
            theIndex.Predicate = "foo > 1";
            theIndex.FillFactor = 70;
            
            theIndex.ToDDL(parent)
                .ShouldBe("CREATE INDEX idx_1 ON public.people USING gin (column1) WHERE (foo > 1) WITH (fillfactor='70');");
        }
        
        [Fact]
        public void generate_ddl_for_descending_sort_order()
        {
            theIndex.SortOrder = SortOrder.Desc;

            theIndex.ToDDL(parent)
                .ShouldBe("CREATE INDEX idx_1 ON public.people USING btree (column1 DESC);");
        }


        public static IEnumerable<object[]> Indexes()
        {
            yield return new[]{new IndexDefinition("idx_1").AgainstColumns("name")};
            yield return new[]{new IndexDefinition("idx_1").AgainstColumns("name", "age")};
            yield return new[]{new IndexDefinition("idx_1")
            {
                IsUnique = true
            }.AgainstColumns("name", "age")};
            
            yield return new[]{new IndexDefinition("idx_1"){SortOrder = SortOrder.Desc}.AgainstColumns("name")};
        }

        [Theory]
        [MemberData(nameof(Indexes))]
        public void IndexParsing(IndexDefinition expected)
        {
            var table = new Table("people");
            var ddl = expected.ToDDL(table);

            var actual = IndexDefinition.Parse(ddl);
            
            expected.AssertMatches(actual, table);

        }

        
    }
}