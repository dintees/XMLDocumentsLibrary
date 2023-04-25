using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLDocumentLibrary;

namespace XMLDocumentApp.Tests
{
    public class QueryBuilderTests
    {
        [Fact]
        public void Should_ReturnXQueryString_When_MethodApplicationIsCorrect()
        {
            QueryBuilder builder = new QueryBuilder();
            builder.GoTo("catalog").GoTo("book");
            Assert.Equal("/catalog/book", builder.ToString());
        }

        [Fact]
        public void Should_ReturnXQueryString_When_XQueryStartsWithRootPath()
        {
            QueryBuilder builder = new QueryBuilder("book", false);
            builder.GoTo("title");
            Assert.Equal("//book/title", builder.ToString());
        }

        [Fact]
        public void Should_ReturnXQueryString_When_XQueryHasIndexedValues()
        {
            QueryBuilder builder = new QueryBuilder("catalog");
            builder.GoTo("book").At(2).GoTo("title");
            Assert.Equal("/catalog/book[2]/title", builder.ToString());
        }

        [Fact]
        public void Should_ReturnXQueryString_When_XQueryHasAttributWithValue()
        {
            QueryBuilder builder = new QueryBuilder("catalog");
            builder.GoTo("book").WithAttribute("id", "bk_105").GoTo("title");
            Assert.Equal("/catalog/book[@id=bk_105]/title", builder.ToString());
        }

    }
}
