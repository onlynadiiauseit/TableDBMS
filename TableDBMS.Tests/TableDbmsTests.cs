using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TableDBMS.DataTypes;
using TableDBMS.Models;
using TableDBMS.Services;

namespace TableDBMS.Tests
{
    [TestClass]
    public class TableDbmsTests
    {
        [TestMethod]
        public void IntegerType_ValidAndInvalidValues()
        {
            var type = new IntegerType();

            Assert.IsTrue(type.IsValid("123"));
            Assert.IsTrue(type.IsValid("-45"));

            Assert.IsFalse(type.IsValid("12.5"));
            Assert.IsFalse(type.IsValid("abc"));
        }

        [TestMethod]
        public void ColorType_ValidAndInvalidValues()
        {
            var type = new ColorType();

            Assert.IsTrue(type.IsValid("#FF0000"));
            Assert.IsTrue(type.IsValid("#00FF7F"));
            Assert.IsTrue(type.IsValid("#000000"));

            Assert.IsFalse(type.IsValid("red"));
            Assert.IsFalse(type.IsValid("FF0000"));
            Assert.IsFalse(type.IsValid("#GG0000"));
            Assert.IsFalse(type.IsValid("#123"));
        }

        [TestMethod]
        public void ColorIntervalType_ValidatesInterval()
        {
            var type = new ColorIntervalType();

            Assert.IsTrue(
                type.IsValid("#000000..#FFFFFF")
            );

            Assert.IsTrue(
                type.IsValid("#000000..#FF0000")
            );

            Assert.IsFalse(
                type.IsValid("#FFFFFF..#000000")
            );

            Assert.IsFalse(
                type.IsValid("red..blue")
            );
        }

        [TestMethod]
        public void Table_AddInvalidRow_ThrowsException()
        {
            var schema = new TableSchema();

            schema.AddColumn(
                new Column(
                    "Id",
                    "Integer"
                )
            );

            schema.AddColumn(
                new Column(
                    "Name",
                    "String"
                )
            );

            var table =
                new Table(
                    "Students",
                    schema
                );

            var invalidRow =
                new Row(
                    new string?[]
                    {
                        "abc",
                        "Anna"
                    }
                );

            Assert.ThrowsExactly<InvalidOperationException>(
                () => table.AddRow(invalidRow)
            );

            Assert.AreEqual(
                0,
                table.Rows.Count
            );
        }

        [TestMethod]
        public void TableUnion_CompatibleTables_ReturnsUnion()
        {
            Table first =
                CreateStudentTable(
                    "Students1"
                );

            first.AddRow(
                new Row(
                    new string?[]
                    {
                        "1",
                        "Anna",
                        "92.5",
                        "KN-21"
                    }
                )
            );

            Table second =
                CreateStudentTable(
                    "Students2"
                );

            second.AddRow(
                new Row(
                    new string?[]
                    {
                        "2",
                        "Ivan",
                        "88.5",
                        "KN-22"
                    }
                )
            );

            var service =
                new TableUnionService();

            Table result =
                service.Union(
                    first,
                    second,
                    "AllStudents"
                );

            Assert.AreEqual(
                "AllStudents",
                result.Name
            );

            Assert.AreEqual(
                2,
                result.Rows.Count
            );

            Assert.AreEqual(
                "Anna",
                result.Rows[0].Values[1]
            );

            Assert.AreEqual(
                "Ivan",
                result.Rows[1].Values[1]
            );
        }

        [TestMethod]
        public void TableUnion_DuplicateRows_AreNotDuplicated()
        {
            Table first =
                CreateStudentTable(
                    "Students1"
                );

            Table second =
                CreateStudentTable(
                    "Students2"
                );

            var firstRow =
                new Row(
                    new string?[]
                    {
                        "1",
                        "Anna",
                        "92.5",
                        "KN-21"
                    }
                );

            var duplicateRow =
                new Row(
                    new string?[]
                    {
                        "1",
                        "Anna",
                        "92.5",
                        "KN-21"
                    }
                );

            first.AddRow(firstRow);
            second.AddRow(duplicateRow);

            var service =
                new TableUnionService();

            Table result =
                service.Union(
                    first,
                    second,
                    "UnionResult"
                );

            Assert.AreEqual(
                1,
                result.Rows.Count
            );

            Assert.AreEqual(
                "Anna",
                result.Rows[0].Values[1]
            );
        }

        [TestMethod]
        public void TableUnion_IncompatibleTables_ThrowsException()
        {
            Table first =
                CreateStudentTable(
                    "Students"
                );

            var otherSchema =
                new TableSchema();

            otherSchema.AddColumn(
                new Column(
                    "Id",
                    "Integer"
                )
            );

            otherSchema.AddColumn(
                new Column(
                    "Description",
                    "String"
                )
            );

            var second =
                new Table(
                    "OtherTable",
                    otherSchema
                );

            var service =
                new TableUnionService();

            Assert.ThrowsExactly<InvalidOperationException>(
                () =>
                    service.Union(
                        first,
                        second,
                        "Result"
                    )
            );
        }

        private static Table CreateStudentTable(
            string tableName)
        {
            var schema =
                new TableSchema();

            schema.AddColumn(
                new Column(
                    "Id",
                    "Integer"
                )
            );

            schema.AddColumn(
                new Column(
                    "Name",
                    "String"
                )
            );

            schema.AddColumn(
                new Column(
                    "Average",
                    "Real"
                )
            );

            schema.AddColumn(
                new Column(
                    "Group",
                    "String"
                )
            );

            return new Table(
                tableName,
                schema
            );
        }
    }
}