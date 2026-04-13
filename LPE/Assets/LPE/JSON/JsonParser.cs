using System.Text;


namespace LPE.JSON {
    public struct JsonParser {
        int currentPointer;
        int line;
        int linePos;
        string src;
        StringBuilder subString;

        public JSONObject Parse(string src) {
            subString ??= new StringBuilder();
            this.src = src;
            currentPointer = 0;
            line = 0;
            linePos = 0;

            subString.Clear();

            return ParseValue().AsObject();
        }

        //**********************************************************************************************
        // States
        //**********************************************************************************************

        JSONValue ParseValue(bool startOfArray = false) {
            JSONValue result = new JSONValue();
            SkipWhiteSpace();

            //float 
            if (StartOfFloat(Peek())) {
                result.SetValue(ParseFloat());
            }
            //string 
            else if (Peek() == '"') {
                result.SetValue(ParseString());
            }
            //true
            else if (Peek(0) == 't' && Peek(1) == 'r' && Peek(2) == 'u' && Peek(3) == 'e') {
                Next();
                Next();
                Next();
                Next();
                result.SetValue(true);
            }
            //false
            else if (Peek(0) == 'f' && Peek(1) == 'a' && Peek(2) == 'l' && Peek(3) == 's' && Peek(4) == 'e') {
                Next();
                Next();
                Next();
                Next();
                Next();
                result.SetValue(false);
            }
            //JSONObject
            else if (Peek() == '{') {
                result.SetAsObject(ParseObject());
            }
            //JsonArray
            else if (Peek() == '[') {
                result.SetAsArray(ParseArray());
            }
            //null
            else if (Peek(0) == 'n' && Peek(1) == 'u' && Peek(2) == 'l' && Peek(3) == 'l') {
                Next();
                Next();
                Next();
                Next();
                result.SetAsNull();
            }   
            else {
                throw new JSONException($"Unexpected '{Peek()}' on line {line}.{linePos}");
            }
            return result;
        }

        JsonArray ParseArray() {
            JsonArray result = new JsonArray();

            // consume '['
            Next();
            // check for Empty Array
            SkipWhiteSpace();
            if (Peek() == ']') {
                // consume
                Next();
                return result;
            }

            // main loop
            while (true) {
                // get value
                SkipWhiteSpace();
                JSONValue val = ParseValue();

                // add to object
                result.entries.Add(val);


                // check for ']'
                SkipWhiteSpace();
                if (Peek() == ']') {
                    // consume
                    Next();

                    // done
                    break;
                }

                // check for ','
                if (Peek() != ',') {
                    throw new JSONException($"Expected ']' on line {line}.{linePos}");
                }

                // consume ','
                Next();
            }

            return result;
        }

        float ParseFloat() {
            SkipWhiteSpace();

            // check for negative
            bool isNegative = Peek() == '-';
            if (isNegative) {
                // consume
                Next();
            }

            // double because char.GetNumericValue() returns a double
            // also more accurate result
            double result = 0;

            int pos = 0; // position after decimal place
            bool point = false; // has a deicimal pont been found?

            // main loop
            while(true) {
                // get next digit
                char c = Peek();
                double d = char.GetNumericValue(c);


                if (d != -1) {
                    // found a digit
                    if (point) {
                        // shift digit to correct decimal place
                        for (int i = 0; i < pos; i++) {
                            d /= 10.0;
                        }
                        result += d;
                        pos++;
                    }
                    else {
                        // shift existing result left 1 (123 => 1230)
                        result *= 10;
                        result += d;
                    }
                }
                else if (!point && c == '.') {
                    // found decimal, only once
                    point = true;
                    pos = 1;
                }
                else {
                    break;
                }

                // consume char, but not if invalid char found
                Next();
            }

            return (float)(isNegative ? -result : result);
        }

        JSONObject ParseObject() {
            JSONObject result = new JSONObject();
            // consume '{'
            Next();

            // check for '}' ie=> empty object
            SkipWhiteSpace();
            if (Peek() == '}') {
                // consume
                Next();

                // done
                return result;
            }

            while(true) {
                // get key
                SkipWhiteSpace();
                string key = ParseString();

                // check for ':'
                SkipWhiteSpace();
                if (Peek() != ':') {
                    throw new JSONException($"Expected ':' on line {line}.{linePos}");
                }
                Next();

                // get value
                SkipWhiteSpace();
                JSONValue val = ParseValue();

                // add to object
                result.SetKV(key, val);


                // check for '}'
                SkipWhiteSpace();
                if (Peek() == '}') {
                    // consume
                    Next();

                    // done
                    break;
                }

                // check for ','
                if (Peek() != ',') {
                    throw new JSONException($"Expected '{'}'}' on line {line}.{linePos}. Got: {Peek()}");
                }

                // consume ','
                Next();
            }

            return result;
        }

        string ParseString() {
            // consume '"'
            if (Peek() != '"') {
                throw new JSONException($"Expected '\"' on line {line}.{linePos}");
            }
            Next();

            // setup
            subString.Clear();
            bool escape = false;

            // main loop
            while (true) {
                char c = Peek();
                // check for '"'
                if (!escape && c == '"') {
                    Next();
                    break;
                }

                // check for escape
                escape = c == '\\';

                // add to substring
                subString.Append(c);
                Next();
            }

            return subString.ToString();

        }
       
        //**********************************************************************************************
        // Helper Functions
        //**********************************************************************************************

        void SkipWhiteSpace() {
            while(true) {
                char c = Peek();

                bool isWhiteSpace = c == ' '
                                 || c == '\n' 
                                 || c == '\r' 
                                 || c == '\t';

                if (!isWhiteSpace) {
                    break;
                }

                Next();
            }
        }

        void Next() {
            if (Peek() == '\n') {
                line++;
                linePos = 0;
            }
            else {
                linePos++;
            }
            currentPointer++;
        }

        char Peek(int count = 0) {
            return src[currentPointer + count];
        }

        bool StartOfFloat(char c) {
            return
                c == '-' ||
                c == '.' ||
                c == '0' ||
                c == '1' ||
                c == '2' ||
                c == '3' ||
                c == '4' ||
                c == '5' ||
                c == '6' ||
                c == '7' ||
                c == '8' ||
                c == '9';
        }
    }
}
