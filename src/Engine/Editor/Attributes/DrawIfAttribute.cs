using System;

namespace ZargoEngine.Editor.Attributes
{
    public enum Condition
    {
        equal, notEqual, bigger, smaller
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DrawIfAttribute : Attribute
    {
        public string methodName;
        public object otherObject;
        public Condition condition;

        public bool Proceed(object value, object value1)
        {
            if (condition == Condition.smaller || condition == Condition.bigger)
            {
                if (value is float float1 && value1 is float float2)
                {
                    if (condition == Condition.smaller) return float1 < float2;
                    else                                return float1 > float2;

                }
                else if (value is int int1 && value1 is int int2)
                {
                    if (condition == Condition.smaller) return int1 < int2;
                    else                                return int1 > int2;
                }
            }

            return condition switch
            {
                Condition.equal    => value.Equals(value1),
                Condition.notEqual => !value.Equals(value1),
            };
        }

        public DrawIfAttribute(string methodName,Condition condition = Condition.equal,object otherObject = null)
        {
            this.methodName = methodName;
            this.condition = condition;
            this.otherObject = otherObject;
        }
    }
}
