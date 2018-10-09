using System;

namespace Chef.Data
{
    public abstract class Field
    {
        public abstract Type GetValueType();

        public abstract object GetValue();
    }

    public class Field<T> : Field
    {
        public Field(T value)
        {
            this.Value = value;
        }

        public T Value { get; }

        public static implicit operator Field<T>(T value)
        {
            return new Field<T>(value);
        }

        public static implicit operator T(Field<T> field)
        {
            return field.Value;
        }

        public override Type GetValueType()
        {
            return typeof(T);
        }

        public override object GetValue()
        {
            return this.Value;
        }
    }
}