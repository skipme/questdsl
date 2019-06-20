using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace questdsl
{
    public partial class Parser
    {


            /*
            comment                 //comment           -
            arg simlink             --a b               a: argN; b: localvar($name)
            section break           \n                  emptyline
            section separator       ----
            substate declaration    a:b                 a: subStateName; b: initialValue(\d; "multiline"; leftrightTrimmedString)

            section body
            conditions
            a==b                     any
            a!=b                     any
            a>b                      any but digit
            a<b                      any but digit
            a<=b                     any but digit
            a>=b                     any but digit

            executives
            a=b                     any                     a only reference
            a+=b                    any but digit           a only reference
            a-=b                    any but digit           a only reference
            a++                     any but digit           a only reference
            a--                     any but digit           a only reference
            ->a b b b               invoke a with b args

            section body reference and values
            a.b
            $a.b
            a.$b
            $a.$b

            \d
            "multiline"
            leftrightTrimmedString

            */
    
        public enum LineType
        {
            comment,
            simlink,
            empty,
            undetermined,
            section_separator,
            substate_declaration,
            condition,
            executive,
            executive_invocation
        }

        public LineType EvaluateLineType(string line)
        {
            // ^\s*\\\\(.*)$|^\s*--\s*(arg\d+)\s*(.+)$|^\s*(-{3,10})\s*$|^\s*(.*)\s*:\s*(.*)$
            /*
Test	Target String	Match()	Result()	Groups[0]	Groups[1]	Groups[2]	Groups[3]	Groups[4]	Groups[5]	Groups[6]
1	    \\ comment  	Yes 	            \\ comment	comment					
2	    --arg1 test 	Yes	                --arg1 test			    arg1		test			
3	    ----	        Yes	                ----								            ----		
4	    expr: $epx	    Yes	                expr: $epx									                expr		$epx
 */
            return EvaluateLineType(line, null);
        }
        public LineType EvaluateLineType(string line, Dictionary<string, string> parserGroups)
        {
            if (string.IsNullOrWhiteSpace(line))
                return LineType.empty;

            Match m = Regex.Match(line, @"^\s*\\(.*)$|^\s*--\s*arg(\d+)\s*\$?(.+)$|^\s*(-{3,10})\s*$|^\s*(.*)\s*:\s*(.*)$|^([\w\.\$]+)\s*(==|<|>|!=|>=|<=)\s*([\w\.\$]+)$|^([\w\.\$]+)\s*(=|\+=|-=)\s*([\w\.\$]+)$|^(.*)\s*(\+\+|--)\s*$|^-->\s*([\w\.\$]*)(\s+.*)*$");
            if (m.Success)
            {
                if (m.Groups[1].Success)
                {
                    if (parserGroups != null)
                        parserGroups.Add("comment", m.Groups[1].Value);
                    return LineType.comment;
                }

                if (m.Groups[2].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("arg", m.Groups[2].Value);
                        parserGroups.Add("sim", m.Groups[3].Value);
                    }
                    return LineType.simlink;
                }
                if (m.Groups[4].Success)
                {
                    return LineType.section_separator;
                }
                if (m.Groups[6].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("substate", m.Groups[5].Value);
                        parserGroups.Add("value", m.Groups[6].Value);
                    }
                    return LineType.substate_declaration;
                }
                if (m.Groups[9].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("left", m.Groups[7].Value);
                        parserGroups.Add("cond", m.Groups[8].Value);
                        parserGroups.Add("right", m.Groups[9].Value);
                    }
                    return LineType.condition;
                }
                if (m.Groups[12].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("left", m.Groups[10].Value);
                        parserGroups.Add("cond", m.Groups[11].Value);
                        parserGroups.Add("right", m.Groups[12].Value);
                    }
                    return LineType.executive;
                }
                if (m.Groups[14].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("left", m.Groups[13].Value);
                        parserGroups.Add("cond", m.Groups[14].Value);
                    }
                    return LineType.executive;
                }
                if (m.Groups[15].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("transName", m.Groups[15].Value);
                        parserGroups.Add("args", m.Groups[16].Value);
                    }
                    return LineType.executive_invocation;
                }
            }
            return LineType.undetermined;
        }
        public enum PartType
        {
            substate,           //xxx.yyy
            substate_subVar,    //xxx.$yyy
            substate_stateVar,  //$xxx.yyy
            substate_allVar,    //$xxx.$yyy
            digit,
            text_string,
            text_multiline,         //"xxx"
            text_multiline_start,   //"...
            text_multiline_end,     //..."

            variable                //$xxx
        }
        public PartType EvaluatePartType(string line)
        {
            return EvaluatePartType(line, null);
        }
        public PartType EvaluatePartType(string line, Dictionary<string, string> parserGroups)
        {
            Match m = Regex.Match(line, @"^\s*(\w+)\.(\w+)\s*$|^\s*(\w+)\.\$(\w+)\s*$|^\s*\$(\w+)\.(\w+)\s*$|^\s*\$(\w+)\.\$(\w+)\s*$|^\s*(\d+)\s*$|^\s*""(.*)""\s*$|^""(.*[^""])$|^([^""].*)""$|^\s*\$(\w+)$");
            if (m.Success)
            {
                if (m.Groups[2].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("left", m.Groups[1].Value);
                        parserGroups.Add("right", m.Groups[2].Value);
                    }
                    return PartType.substate;
                }
                if (m.Groups[4].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("left", m.Groups[3].Value);
                        parserGroups.Add("right", m.Groups[4].Value);
                    }
                    return PartType.substate_subVar;
                }
                if (m.Groups[6].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("left", m.Groups[5].Value);
                        parserGroups.Add("right", m.Groups[6].Value);
                    }
                    return PartType.substate_stateVar;
                }
                if (m.Groups[8].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("left", m.Groups[7].Value);
                        parserGroups.Add("right", m.Groups[8].Value);
                    }
                    return PartType.substate_allVar;
                }
                if (m.Groups[9].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("number", m.Groups[9].Value);
                    }
                    return PartType.digit;
                }
                if (m.Groups[10].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("string", m.Groups[10].Value);
                    }
                    return PartType.text_multiline;
                }
                if (m.Groups[11].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("string", m.Groups[11].Value);
                    }
                    return PartType.text_multiline_start;
                }
                if (m.Groups[12].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("string", m.Groups[12].Value);
                    }
                    return PartType.text_multiline_end;
                }
                if (m.Groups[13].Success)
                {
                    if (parserGroups != null)
                    {
                        parserGroups.Add("var", m.Groups[13].Value);
                    }
                    return PartType.variable;
                }
            }
            return PartType.text_string;
        }
    }
}
