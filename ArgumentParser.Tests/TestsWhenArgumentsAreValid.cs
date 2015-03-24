﻿using System;
using ArgumentParser.App;
using NUnit.Framework;

namespace ArgumentParser.Tests
{
	[TestFixture]
	public class TestsWhenArgumentsAreValid
	{
		private string[] _emptyArgs;

        [SetUp]
        public void SetUp()
        {
            _emptyArgs = new string[] {};
        }

		[Test]
		public void Should_set_boolean_to_true_when_it_is_passed_in()
		{
			var args = new[] { "-l" };
			var arg = new Args("l", args);
			Assert.That(arg.GetBoolean('l'), Is.True);
			Assert.That(arg.IsValid(), Is.True);
		}

		[Test]
		public void Should_set_boolean_to_false_when_it_is_not_passed_in()
		{
			var arg = new Args("l", _emptyArgs);
			Assert.That(arg.GetBoolean('l'), Is.False);
			Assert.That(arg.IsValid(), Is.True);
		}

		[Test]
		public void Should_set_integer_when_it_is_passed_in()
		{
			var args = new[] { "-p", "3000" };
			var arg = new Args("p#", args);
			Assert.That(arg.GetInt('p'), Is.EqualTo(3000));
			Assert.That(arg.IsValid(), Is.True);
		}

		[Test]
		public void Should_set_integer_to_zero_when_no_value_is_passed_in()
		{
			var arg = new Args("p#", _emptyArgs);
			Assert.That(arg.GetInt('p'), Is.EqualTo(0));
			Assert.That(arg.IsValid(), Is.True);
		}

		[Test]
		public void Should_set_string_when_it_is_passed_in()
		{
			var args = new[] { "-d", "myPath" };
			var arg = new Args("d*", args);
			Assert.That(arg.GetString('d'), Is.EqualTo("myPath"));
			Assert.That(arg.IsValid(), Is.True);
		}

		[Test]
		public void Should_set_string_to_empty_when_no_value_is_passed_in()
		{
			var arg = new Args("d*", _emptyArgs);
			Assert.That(arg.GetString('d'), Is.EqualTo(""));
			Assert.That(arg.IsValid(), Is.True);
		}

		[Test]
		public void Should_set_cardinality()
		{
			var args = new[] {"-l", "-d", "test"};
			var arg = new Args("n#,d*,l", args);
			Assert.That(arg.Cardinality(), Is.EqualTo(2));
		}

		[Test]
		public void Should_track_usage_when_schema_is_supplied()
		{
			var arg = new Args("n#", _emptyArgs);
			Assert.That(arg.Usage(), Is.EqualTo("-[n#]"));
		}

		[Test]
		public void Should_track_usage_when_no_schema_is_supplied()
		{
			var arg = new Args("", _emptyArgs);
			Assert.That(arg.Usage(), Is.EqualTo(""));
		}

		[Test]
		public void Should_identify_elements_of_the_schema_that_are_used()
		{
			var args = new[] { "-l", "-d", "test"};
			var arg = new Args("l,d*,n#", args);
			Assert.True(arg.Has('l'));
			Assert.True(arg.Has('d'));
			Assert.False(arg.Has('n'));
		}

		[Test]
		public void Should_throw_exception_when_error_message_is_requested_for_valid_input()
		{
			var arg = new Args("l,d*,n#", _emptyArgs);
			Assert.True(arg.IsValid());
			var error = Assert.Throws<Exception>(() => arg.ErrorMessage());
			Assert.That(error.Message, Is.EqualTo("TILT : Should not get here."));
		}

        [Test]
		public void Should_return_false_if_asking_for_non_existant_boolean_argument()
		{
			var arg = new Args("a", _emptyArgs);
		    Assert.False(arg.GetBoolean('b'));
        }

        [Test]
		public void Should_return_empty_string_if_asking_for_non_existant_string_argument()
		{
			var arg = new Args("a*", _emptyArgs);
		    Assert.That(arg.GetString('b'), Is.EqualTo(String.Empty));
        }  
        
        [Test]
		public void Should_return_0_if_asking_for_non_existant_int_argument()
		{
			var arg = new Args("a#", _emptyArgs);
		    Assert.That(arg.GetInt('b'), Is.EqualTo(0));
        }

        [Test]
		public void Should_return_false_if_checking_whether_non_existant_boolean_argument_exists()
		{
			var arg = new Args("a", _emptyArgs);
            Assert.False(arg.IsBooleanArg('b'));
        }

        [Test]
        public void Should_return_false_if_checking_whether_non_existant_string_argument_exists()
        {
            var arg = new Args("a*", _emptyArgs);
            Assert.False(arg.IsStringArg('b'));
        }  
        
        [Test]
        public void Should_return_false_if_checking_whether_non_existant_int_argument_exists()
        {
            var arg = new Args("a#", _emptyArgs);
            Assert.False(arg.IsIntArg('b'));
        }

        [Test]
        public void Should_return_empty_string_from_new_argument_marshaller()
        {
            var stringArgumentMarshaller = new ArgumentMarshaller<string>{Value = string.Empty};
            Assert.That(stringArgumentMarshaller.Value, Is.EqualTo(string.Empty));
        }
	}
}