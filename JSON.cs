using System;
using System.Collections.Generic;


/// <summary>
///     JSON is a c# repersentation of JavaScript Object
/// </summary>
public class JSON
{
    /// <summary>
    ///     type object defines whether a JSON object is a string, dictionary or list
    /// </summary>
    public readonly string type;
    
    /// <summary>
    ///     obj stores JSON data
    /// </summary>
    public object obj;

    /// <summary>
    ///     Constructor creates JSON object during parsing
    /// </summary>
    /// <param name="type"></param>
    /// <param name="obj"></param>
    private JSON(string type, object obj)
    {
        this.type = type;
        this.obj = obj;
    }


    /// <summary>
    ///     ToString recursively converts JSON object into string repersentation
    /// </summary>
    /// <returns>String of JSON object</returns>
    public override string ToString()
    {

        string tmp = "";
        if (this.type == typeof(string).ToString())
        {
            tmp += $"\"{(string)this.obj}\"";
        }
        else if(this.type == typeof(Array).ToString())
        {
            JSON[] p = (JSON[])this.obj;
            tmp += "[";
            for(int i1 = 0; i1 < p.Length; i1++)
            {
                tmp += $"{p[i1]},";
            }
            tmp = tmp.Substring(0, tmp.Length - 1) + "]";
        }
        else if(this.type == typeof(Dictionary<string, JSON>).ToString())
        {
            Dictionary<string, JSON> p = (Dictionary<string, JSON>)this.obj;
            tmp += "{";
            foreach (string key in p.Keys)
            {
                tmp += $"\"{key}\":{p[key]},";
            }
            tmp = tmp.Substring(0, tmp.Length - 1) + "}";
        }

        return tmp;
    }

    /// <summary>
    ///     ParseException handles throwing specfic exceptions related to parsing JSON strings
    /// </summary>
    private class ParseException : Exception
    {
        public ParseException()
            : base("Invalid file format\nRan out of Chars without closing arrays/strings/dictionaries")
        {

        }
        public ParseException(string location, string unexpectedToken)
            : base($"Invalid file format\nLocation:{location}\tunexpectedToken:'{unexpectedToken}'")
        {

        }
    }

    /// <summary>
    ///     parse is a static method that converts a string into a JSON object
    /// </summary>
    /// <param name="raw"></param>
    /// <returns>JSON object of raw</returns>
    public static JSON parse(string raw)
    {
        List<char> chars = new List<char>(raw.ToCharArray());
        JSON tmp;
        switch(chars[0])
        {
            case '{':
                chars.RemoveAt(0);
                tmp = dictonaryParse(chars);
                break;
            case '[':
                chars.RemoveAt(0);
                tmp = arrayParse(chars);
                break;
            default:
                throw new ParseException("Parse",$"{chars[0]}");
        }
        return tmp;
    }

    /// <summary>
    ///     dictonaryParse is a private static method that recusively parses a dictionary
    /// </summary>
    /// <param name="chars">list of every char left to parse</param>
    /// <returns>Dictionary JSON object</returns>
    private static JSON dictonaryParse(List<char> chars)
    {
        Dictionary<string, JSON> dictonaryTmp = new Dictionary<string, JSON>();
        bool defineCond = false;

        JSON dictValue = null;
        JSON tmp = null;
        string lastKey = null;

        char charTmp;
        while (chars.Count > 0)
        {
            charTmp = chars[0];
            chars.RemoveAt(0);
            switch (charTmp)
            {
                //invalid file format
                case ']'://closing dictionary
                    throw new ParseException("dictonaryParse", $"{charTmp}");

                //close dictonary
                case '}':
                    if(defineCond)
                    {
                        if (dictValue != null && lastKey != null)
                        {
                            dictonaryTmp[lastKey] = dictValue;
                        }
                        else
                        {
                            throw new ParseException("dictonaryParse", $"{charTmp}");
                        }
                    }
                    
                    return new JSON(typeof(Dictionary<string, JSON>).ToString(), dictonaryTmp);

                //Adding key
                case ':':
                    // "a" : "b" :
                    //           ^ double define
                    if (defineCond)
                    {
                        throw new ParseException("dictonaryParse", $"{charTmp}");
                    }
                    else
                    {
                        defineCond = true;
                        dictonaryTmp.Add(lastKey, null);
                    }
                    break;
                case ',':
                    //checking valid define term
                    // "a" : "b",
                    //     ^ checking if define set
                    // "a" : b,
                    //       ^ not defined
                    // "a" : b,
                    //  ^ not defined
                    if (!defineCond || dictValue == null || lastKey == null)
                    {
                        throw new ParseException("dictonaryParse", $"{charTmp}");
                    }
                    else
                    {
                        defineCond = false;
                        dictonaryTmp[lastKey] = dictValue;
                        dictValue = null;
                        lastKey = null;
                        //define last element
                    }
                    break;
                //adding to dictionary
                case '\"'://new string
                    tmp = stringParse(chars);
                    if(!defineCond && lastKey == null)
                    {
                        lastKey = (string) tmp.obj;
                    }
                    else if(defineCond && dictValue == null)
                    {
                        dictValue = tmp;
                    }
                    else
                    {
                        throw new ParseException("dictonaryParse", $"{charTmp}");
                    }
                    break;
                case '{'://new dictionary
                    tmp = dictonaryParse(chars);
                    
                    if (defineCond && dictValue == null)
                    {
                        dictValue = tmp;
                    }
                    else
                    {
                        throw new ParseException("dictonaryParse", $"{charTmp}");
                    }
                    break;
                case '['://new array
                    tmp = arrayParse(chars);

                    if (defineCond && dictValue == null)
                    {
                        dictValue = tmp;
                    }
                    else
                    {
                        throw new ParseException("dictonaryParse", $"{charTmp}");
                    }
                    break;
            }
        }

        throw new ParseException();
    }

    /// <summary>
    ///     arrayParse is a private static method that recusively parses an array
    /// </summary>
    /// <param name="chars">list of every char left to parse</param>
    /// <returns>Array JSON object</returns>
    private static JSON arrayParse(List<char> chars)
    {
        List<JSON> arrayTmp = new List<JSON>();
        JSON tmp = null;
        char charTmp;
        while (chars.Count > 0)
        {
            charTmp = chars[0];
            chars.RemoveAt(0);
            switch (charTmp)
            {
                //invalid file format
                case ':'://defing dictionary value
                case '}'://closing dictionary
                    throw new ParseException("arrayParse", $"{charTmp}");
                
                //close array
                case ']':
                    if (tmp != null)
                    {
                        arrayTmp.Add(tmp);
                    }
                    return new JSON(typeof(Array).ToString(), arrayTmp.ToArray());
                
                //adding to array
                case '\"'://new string
                    if(tmp != null)
                    {
                        throw new ParseException("arrayParse", $"{charTmp}");
                    }
                    tmp = stringParse(chars);
                    break;
                case '{'://new dictionary
                    if (tmp != null)
                    {
                        throw new ParseException("arrayParse", $"{charTmp}");
                    }
                    tmp = dictonaryParse(chars);
                    break;
                case '['://new array
                    if (tmp != null)
                    {
                        throw new ParseException("arrayParse", $"{charTmp}");
                    }
                    tmp = arrayParse(chars);
                    break;
                case ',':
                    if (tmp == null)
                    {
                        throw new ParseException("arrayParse", $"{charTmp}");
                    }
                    arrayTmp.Add(tmp);
                    tmp = null;
                    break;
            }
        }

        throw new ParseException();
    }


    /// <summary>
    ///     stringParse is a private static method that parses set of char into a string
    /// </summary>
    /// <param name="chars">list of every char left to parse</param>
    /// <returns>string JSON object</returns>
    private static JSON stringParse(List<char> chars)
    {
        char tmp;
        string strTmp = "";
        while (chars.Count > 0)
        {
            tmp = chars[0];
            if(tmp == '\"')
            {
                chars.RemoveAt(0);
                return new JSON(typeof(string).ToString(), strTmp);
            }
            strTmp += $"{tmp}";
            chars.RemoveAt(0);
        }

        throw new ParseException();
    }
}
