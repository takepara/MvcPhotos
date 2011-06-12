using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvcPhotos.Tests
{
    [TestClass]
    public class ObjectResolverTest
    {
        [TestInitialize]
        public void Initialize()
        {
            ObjectResolver.Clear();
        }

        [TestMethod]
        public void 文字列キーに整数１つ()
        {
            ObjectResolver.Entry("文字列キーに整数１つ", () => 100);

            var value = ObjectResolver.Resolve<int>("文字列キーに整数１つ");
            Assert.AreEqual(value, 100);
        }

        [TestMethod]
        public void 文字列キーに整数３つ()
        {
            ObjectResolver.Entry("文字列キーに整数３つ", () => 100);
            ObjectResolver.Entry("文字列キーに整数３つ", () => 200);
            ObjectResolver.Entry("文字列キーに整数３つ", () => 300);

            var values = ObjectResolver.Resolves<int>("文字列キーに整数３つ");
            Assert.AreEqual(values.Count(), 3);
            Assert.AreEqual(values.Sum(), 600);
        }

        [TestMethod]
        public void 文字列キーに文字列１つ()
        {
            ObjectResolver.Entry("文字列キーに文字列１つ", () => "resolved!");

            var value = ObjectResolver.Resolve<string>("文字列キーに文字列１つ");
            Assert.AreEqual(value, "resolved!");
        }

        [TestMethod]
        public void 文字列キーに日付１つ()
        {
            ObjectResolver.Entry("文字列キーに日付１つ", () => new DateTime(2000, 1, 1));

            var value = ObjectResolver.Resolve<DateTime>("文字列キーに日付１つ");
            Assert.AreEqual(value, new DateTime(2000, 1, 1));
        }

        [TestMethod]
        public void ジェネリックで整数１つ()
        {
            ObjectResolver.Entry<int>(() => 100);

            var value = ObjectResolver.Resolve<int>();
            Assert.AreEqual(value, 100);
        }

        [TestMethod]
        public void ジェネリックで整数３つ()
        {
            ObjectResolver.Entry<int>(() => 100);
            ObjectResolver.Entry<int>(() => 200);
            ObjectResolver.Entry<int>(() => 300);

            var values = ObjectResolver.Resolves<int>();
            Assert.AreEqual(values.Count(), 3);
            Assert.AreEqual(values.Sum(), 600);
        }

        public interface IPerson
        {
            int Age { get; set; }
            string Name { get; set; }
        }

        public class Employee : IPerson
        {
            public int Age { get; set; }
            public string Name { get; set; }
            public decimal Salary { get; set; }
        }

        public class 能力者 : IPerson
        {
            public int Age { get; set; }
            public string Name { get; set; }
            public string 能力 { get; set; }
        }

        [TestMethod]
        public void 型キーに実装１つインターフェース版()
        {
            ObjectResolver.Entry(typeof (IPerson), () => new Employee {Age = 18, Name = "たけはら"});

            var person = ObjectResolver.Resolve<IPerson>(typeof(IPerson));
            
            Assert.IsNotNull(person);
            Assert.IsInstanceOfType(person,typeof(IPerson));
            Assert.AreEqual(person.Age, 18);
            Assert.AreEqual(person.Name, "たけはら");
        }

        [TestMethod]
        public void 型キーに実装１つクラス版()
        {
            ObjectResolver.Entry(typeof(IPerson), () => new Employee { Age = 18, Name = "たけはら", Salary = 400 });

            var person = ObjectResolver.Resolve<Employee>(typeof(IPerson));

            Assert.IsNotNull(person);
            Assert.IsInstanceOfType(person, typeof(Employee));
            Assert.AreEqual(person.Age, 18);
            Assert.AreEqual(person.Name, "たけはら");
            Assert.AreEqual(person.Salary, 400);
        }

        [TestMethod]
        public void 型キーに実装３つ能力者()
        {
            ObjectResolver.Entry(typeof(IPerson), () => new 能力者 { Name = "赤犬", 能力 = "マグマグ" });
            ObjectResolver.Entry(typeof(IPerson), () => new 能力者 { Name = "青雉", 能力 = "ヒエヒエ" });
            ObjectResolver.Entry(typeof(IPerson), () => new 能力者 { Name = "黄猿", 能力 = "ピカピカ" });

            var people = ObjectResolver.Resolves<能力者>(typeof(IPerson)).ToList();

            Assert.IsNotNull(people);
            Assert.AreEqual(3, people.OfType<能力者>().Count());
            Assert.AreEqual(people[0].能力, "マグマグ");
            Assert.AreEqual(people[1].能力, "ヒエヒエ");
            Assert.AreEqual(people[2].能力, "ピカピカ");
        }

        [TestMethod]
        public void ジェネリックで実装１つインターフェース版()
        {
            ObjectResolver.Entry<IPerson>(() => new Employee { Age = 18, Name = "たけはら" });

            var person = ObjectResolver.Resolve<IPerson>();

            Assert.IsNotNull(person);
            Assert.IsInstanceOfType(person, typeof(IPerson));
            Assert.AreEqual(person.Age, 18);
            Assert.AreEqual(person.Name, "たけはら");
        }

        [TestMethod]
        public void ジェネリックで実装１つクラス版()
        {
            ObjectResolver.Entry<IPerson>(() => new Employee { Age = 18, Name = "たけはら", Salary = 400 });

            var person = ObjectResolver.Resolve<IPerson>();
            var employee = person as Employee;

            Assert.IsNotNull(person);
            Assert.IsNotNull(employee);

            Assert.AreEqual(employee.Age, 18);
            Assert.AreEqual(employee.Name, "たけはら");
            Assert.AreEqual(employee.Salary, 400);
        }

        [TestMethod]
        public void ジェネリックで実装３つ能力者()
        {
            ObjectResolver.Entry<IPerson>(() => new 能力者 { Name = "赤犬", 能力 = "マグマグ" });
            ObjectResolver.Entry<IPerson>(() => new 能力者 { Name = "青雉", 能力 = "ヒエヒエ" });
            ObjectResolver.Entry<IPerson>(() => new 能力者 { Name = "黄猿", 能力 = "ピカピカ" });

            var people = ObjectResolver.Resolves<IPerson>().OfType<能力者>().ToList();

            Assert.IsNotNull(people);
            Assert.AreEqual(3, people.Count());
            Assert.AreEqual(people[0].能力, "マグマグ");
            Assert.AreEqual(people[1].能力, "ヒエヒエ");
            Assert.AreEqual(people[2].能力, "ピカピカ");
        }
    }
}
