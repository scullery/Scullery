using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Scullery.EntityFrameworkCore
{
    public class TestJobs
    {
        public static void Job1(int n)
        {
        }

        public static Task AsyncJob1(int n)
        {
            return Task.CompletedTask;
        }

        public void InstanceJob1(int n)
        {
        }

        public Task InstanceJobAsync1(int n)
        {
            return Task.CompletedTask;
        }

        public static void ParentModelJob1(ParentModel model)
        {
            Assert.Equal("StringValue", model.StringValue);
            Assert.Equal(ushort.MaxValue, model.UShortValue);
            Assert.Equal(short.MinValue, model.ShortValue);
            Assert.Equal(uint.MaxValue, model.UIntValue);
            Assert.Equal(int.MinValue, model.IntValue);
            Assert.Equal(float.MaxValue, model.FloatValue);
            Assert.Equal(double.MinValue, model.DoubleValue);
            Assert.True(model.BoolValue);
            Assert.Equal("ChildValue", model.ChildValue.StringValue);
            Assert.Equal(int.MaxValue, model.ChildValue.IntValue);
            Assert.False(model.ChildValue.BoolValue);
        }

        public static ParentModel CreateParentModel1()
        {
            return new ParentModel
            {
                StringValue = "StringValue",
                UShortValue = ushort.MaxValue,
                ShortValue = short.MinValue,
                UIntValue = uint.MaxValue,
                IntValue = int.MinValue,
                FloatValue = float.MaxValue,
                DoubleValue = double.MinValue,
                BoolValue = true,
                ChildValue = new ChildModel
                {
                    StringValue = "ChildValue",
                    IntValue = int.MaxValue,
                    BoolValue = false
                }
            };
        }

        public static void ErrorJob()
        {
            throw new Exception("Job error");
        }

        public static Task ErrorJobAsync()
        {
            throw new Exception("Async job error");
        }
    }

    public class ParentModel
    {
        public string StringValue { get; set; }
        public short ShortValue { get; set; }
        public ushort UShortValue { get; set; }
        public int IntValue { get; set; }
        public uint UIntValue { get; set; }
        public long LongValue { get; set; }
        public ulong ULongValue { get; set; }
        public float FloatValue { get; set; }
        public double DoubleValue { get; set; }
        public bool BoolValue { get; set; }
        public object[] ArrayValue { get; set; }
        public ChildModel ChildValue { get; set; }
    }

    public class ChildModel
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }
        public object[] ArrayValue { get; set; }
    }
}
