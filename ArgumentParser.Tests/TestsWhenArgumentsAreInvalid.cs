using System;
using ArgumentParser.App;
using NUnit.Framework;

namespace ArgumentParser.Tests
{
    [TestFixture]
    public class TestsWhenArgumentsAreInvalid
    {
        private string[] _emptyArgs;

        [SetUp]
        public void SetUp()
        {
            _emptyArgs = new string[] { };
        }

        [Test]
        public void Should_not_be_valid_if_unknown_argument_is_supplied()
        {
            var args = new[] { "-m" };
            var arg = new Args("l", args);
            Assert.That(arg.IsValid(), Is.False);
            Assert.That(arg.ErrorMessage(), Is.EqualTo("Argument(s) -m unexpected."));
        }

        [Test]
        public void Should_throw_format_exception_when_schema_element_has_incorrect_tail()
        {
            var args = new[] { "-u" };
            var error = Assert.Throws<FormatException>(() => new Args("u=", args));
            Assert.That(error.Message, Is.EqualTo("Argument u has invalid format : ="));
        }

        [Test]
        public void Should_throw_format_exception_when_schema_element_is_not_a_letter()
        {
            var error = Assert.Throws<FormatException>(() => new Args("5*", _emptyArgs));
            Assert.That(error.Message, Is.EqualTo("Bad character: 5 in Args format 5*"));
        }

        [Test]
        public void Should_error_when_string_is_supplied_for_an_integer_value()
        {
            var args = new[] { "-n", "test" };
            var arg = new Args("n#", args);
            Assert.False(arg.IsValid());
            Assert.That(arg.ErrorMessage(), Is.EqualTo("Argument n expects an integer but was TILT"));
        }

        [Test]
        public void Should_error_when_int_value_isnt_supplied()
        {
            var args = new[] { "-n" };
            var arg = new Args("n#", args);
            Assert.False(arg.IsValid());
            Assert.That(arg.ErrorMessage(), Is.EqualTo("Could not find integer parameter for n"));
        }

        [Test]
        public void Should_error_when_a_string_value_isnt_supplied()
        {
            var args = new[] { "-n" };
            var arg = new Args("n*", args);
            Assert.False(arg.IsValid());
            Assert.That(arg.ErrorMessage(), Is.EqualTo("Could not find string parameter for n"));
        }
    }
}
