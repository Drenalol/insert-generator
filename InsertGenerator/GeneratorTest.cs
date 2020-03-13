using System;
using NUnit.Framework;
using static InsertGenerator.Generator;

namespace InsertGenerator
{
    public class GeneratorTest
    {
        [TestCase(null)]
        [TestCase("id")]
        public void InsertGeneratorWithConflictOptionsTest(string returning)
        {
            var insert = GetStringInsertOnConflict<TestInsert>(conflict => new {conflict.Id, conflict.DateTime}, returning);
            Assert.IsFalse(string.IsNullOrEmpty(insert));
            Console.WriteLine(insert);
        }

        [TestCase(null)]
        [TestCase("qwe")]
        public void InsertGeneratorTest(string returning)
        {
            var insert = GetStringInsert<TestInsert>(returning);
            Assert.IsFalse(string.IsNullOrEmpty(insert));
            Console.WriteLine(insert);
        }
    }
}