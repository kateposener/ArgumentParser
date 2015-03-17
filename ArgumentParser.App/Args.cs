﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser.App
{
	public class Args
	{
		private readonly string _schema;
		private readonly string[] _args;
		private bool _valid = true;
		private readonly List<char> _unexpectedArguments = new List<char>();
        private readonly Dictionary<char, BooleanArgumentMarshaller> _booleanArgs = new Dictionary<char, BooleanArgumentMarshaller>();
		private readonly Dictionary<char, StringArgumentMarshaller> _stringArgs = new Dictionary<char, StringArgumentMarshaller>();
		private readonly Dictionary<char, IntArgumentMarshaller> _intArgs = new Dictionary<char, IntArgumentMarshaller>();
		private readonly List<char> _argsFound = new List<char>();
		private int _currentArgument;
		private char _errorArgumentId = '\0';
		private const string ErrorParameter = "TILT";
		private ErrorCode _errorCode = ErrorCode.Ok;
		
		public Args(string schema, string[] args)
		{
			_schema = schema;
			_args = args;
			_valid = Parse();
		}

		private bool Parse()
		{
			if (_schema.Length == 0 && _args.Length == 0)
				return true;
			ParseSchema();
			try
			{
				ParseArguments();
			}
			catch (ArgumentException)
			{
			}
			return _valid;
		}

		private void ParseSchema()
		{
		    foreach (var element in _schema.Split(','))
			{
				if (element.Length > 0)
				{
					var trimmedElement = element.Trim();
					ParseSchemaElement(trimmedElement);
				}
			}
		}

	    private void ParseSchemaElement(string element)
		{
			var elementId = element[0];
			var elementTail = element.Substring(1);
			ValidateSchemaElementId(elementId);
			if (IsBooleanSchemaElement(elementTail))
				ParseBooleanSchemaElement(elementId);
			else if (IsStringSchemaElement(elementTail))
				ParseStringSchemaElement(elementId);
			else if (IsIntegerSchemaElement(elementTail))
				ParseIntegerSchemaElement(elementId);
			else
				throw new FormatException(string.Format("Argument {0} has invalid format : {1}", elementId, elementTail));
		}

		private void ValidateSchemaElementId(char elementId)
		{
			if (!char.IsLetter(elementId))
				throw new FormatException(string.Format("Bad character: {0} in Args format {1}", elementId, _schema));
		}

		private void ParseBooleanSchemaElement(char elementId)
		{
			_booleanArgs.Add(elementId, new BooleanArgumentMarshaller());
		}

		private void ParseIntegerSchemaElement(char elementId)
		{
			_intArgs.Add(elementId, new IntArgumentMarshaller());
		}

		private void ParseStringSchemaElement(char elementId)
		{
			_stringArgs.Add(elementId, new StringArgumentMarshaller());
		}

		private bool IsStringSchemaElement(string elementTail)
		{
			return elementTail.Equals("*");
		}

		private bool IsBooleanSchemaElement(string elementTail)
		{
			return elementTail.Length == 0;
		}

		private bool IsIntegerSchemaElement(string elementTail)
		{
			return elementTail.Equals("#");
		}

		private void ParseArguments()
		{
		    for (_currentArgument = 0; _currentArgument < _args.Length; _currentArgument++)
			{
				var arg = _args[_currentArgument];
				ParseArguments(arg);
			}
		}

	    private void ParseArguments(string arg)
		{
			if (arg.StartsWith("-"))
				ParseElements(arg);
		}

		private void ParseElements(string arg)
		{
			for (var i = 1; i < arg.Length; i++)
				ParseElement(arg[i]);
		}

		private void ParseElement(char argChar)
		{
			if (SetArgument(argChar))
				_argsFound.Add(argChar);
			else
			{
				_unexpectedArguments.Add(argChar);
				_errorCode = ErrorCode.UnexpectedArgument;
				_valid = false;
			}
		}

		private bool SetArgument(char argChar)
		{
			if (IsBooleanArg(argChar))
				SetBooleanArg(argChar, true);
			else if (IsStringArg(argChar))
				SetStringArg(argChar);
			else if (IsIntArg(argChar))
				SetIntArg(argChar);
			else return false;

			return true;
		}

		public bool IsIntArg(char argChar)
		{
			return _intArgs.ContainsKey(argChar);
		}

		private void SetIntArg(char argChar)
		{
			_currentArgument++;
			try
			{
				var parameter = _args[_currentArgument];
				var intArg = int.Parse(parameter);
				if (_intArgs.ContainsKey(argChar))
					_intArgs[argChar].IntValue = intArg;
				else
				{
				    var intArgumentMarshaller = new IntArgumentMarshaller {IntValue = intArg};
				    _intArgs.Add(argChar, intArgumentMarshaller);
				}
			}
			catch (IndexOutOfRangeException)
			{
				_valid = false;
				_errorArgumentId = argChar;
				_errorCode = ErrorCode.MissingInteger;
				throw new ArgumentException();
			}
			catch (FormatException)
			{
				_valid = false;
				_errorArgumentId = argChar;
				_errorCode = ErrorCode.InvalidInterger;
				throw new ArgumentException();
			}
		}

		private void SetStringArg(char argChar)
		{
			_currentArgument++;
			try
			{
				if (_stringArgs.ContainsKey(argChar))
					_stringArgs[argChar].StringValue = _args[_currentArgument];
				else

				{
				    var stringArgumentMarshaller = new StringArgumentMarshaller {StringValue = _args[_currentArgument]};
				    _stringArgs.Add(argChar, stringArgumentMarshaller);
				}
			}
			catch (IndexOutOfRangeException)
			{
				_valid = false;
				_errorArgumentId = argChar;
				_errorCode = ErrorCode.MissingString;
				throw new ArgumentException();
			}
		}

		public bool IsStringArg(char argChar)
		{
			return _stringArgs.ContainsKey(argChar);
		}

		private void SetBooleanArg(char argChar, bool value)
		{
			if (_booleanArgs.ContainsKey(argChar))
				_booleanArgs[argChar].BooleanValue = value;
            else
			{
			    var booleanArgumentMarshaller = new BooleanArgumentMarshaller {BooleanValue = value};
			    _booleanArgs.Add(argChar, booleanArgumentMarshaller);
			}                
		}

		public bool IsBooleanArg(char argChar)
		{
		    return _booleanArgs.ContainsKey(argChar);
		}

		public int Cardinality()
		{
			return _argsFound.Count;
		}

		public string Usage()
		{
		    if (_schema.Length > 0)
				return "-[" + _schema + "]";
		    return "";
		}

	    public string ErrorMessage()
		{
			switch (_errorCode)
			{
				case ErrorCode.Ok:
					throw new Exception("TILT : Should not get here.");
				case ErrorCode.UnexpectedArgument:
					return UnexpectedArgumentMessage();
				case ErrorCode.MissingString:
					return string.Format("Could not find string parameter for {0}", _errorArgumentId);
				case ErrorCode.InvalidInterger:
					return string.Format("Argument {0} expects an integer but was {1}", _errorArgumentId, ErrorParameter);
				case ErrorCode.MissingInteger:
					return string.Format("Could not find integer parameter for {0}", _errorArgumentId);
			}
			return string.Empty;
		}

		private string UnexpectedArgumentMessage()
		{
			var message = new StringBuilder("Argument(s) -");
			foreach (var c in _unexpectedArguments)
			{
				message.Append(c);
			}
			message.Append(" unexpected.");
			return message.ToString();
		}

	    public string GetString(char arg)
	    {
	        try
	        {
	            return _stringArgs[arg].StringValue;
	        }
	        catch (Exception)
	        {
	            return string.Empty;
	        }
	    }

	    public int GetInt(char arg)
	    {
	        IntArgumentMarshaller intArgumentMarshaller;
	        if (_intArgs.TryGetValue(arg, out intArgumentMarshaller))
	            return intArgumentMarshaller.IntValue;
	        return 0;
	    }

		public bool GetBoolean(char arg)
		{
		    BooleanArgumentMarshaller booleanArgumentMarshaller;
		    if (_booleanArgs.TryGetValue(arg, out booleanArgumentMarshaller))
		        return booleanArgumentMarshaller.BooleanValue;
		    return false;
		}

	    public bool Has(char arg)
		{
			return _argsFound.Contains(arg);
		}

		public bool IsValid()
		{
			return _valid;
		}

		private enum ErrorCode
		{
			Ok,
			MissingString,
			MissingInteger,
			InvalidInterger,
			UnexpectedArgument
		}
	}

    public class BooleanArgumentMarshaller
    {
        public bool BooleanValue { get; set; }
    }

    public class StringArgumentMarshaller
    {
        public string StringValue { get; set; }

        public StringArgumentMarshaller()
        {
            StringValue = "";
        }
    }

    public class IntArgumentMarshaller
    {
        public int IntValue { get; set; }
    }
}