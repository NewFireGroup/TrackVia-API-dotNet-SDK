using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace InfiNet.TrackVia.Model
{
    [Serializable]
    public class RecordData : Dictionary<string, object>, IIdentifiable
    {
        public const string INTERNAL_ID_FIELD_NAME = "id";

        public RecordData() : base() { }

        public RecordData(RecordData dictionary) : base(dictionary) { }

        protected RecordData(
           SerializationInfo info, 
           StreamingContext context) : base(info, context) { }

        #region override Equals

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is RecordData)) return false;

            RecordData otherRecord = (RecordData)obj;

            if (this.Count != otherRecord.Count)
                return false;

            foreach(string key in this.Keys)
            {
                object testValue;
                if (!otherRecord.TryGetValue(key, out testValue) || !Equals(testValue, this[key]))
                {
                    if(testValue.GetType() == typeof(string[]) && this[key].GetType() == typeof(string[]))
                    {
                        bool result = ArraysEqual(testValue as string[], this[key] as string[]);
                        return result;
                    }
                    else
                        return false;

                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            foreach (string key in this.Keys)
            {
                var value = this[key];
                var valueHash = value == null ? 0 : value.GetHashCode();
                result ^= ((key.GetHashCode() << 16 | key.GetHashCode() >> 16) ^ prime);
            }
            return result;
        }

        #endregion

        public long Id
        {
            get
            {
                long? result = (this.ContainsKey(INTERNAL_ID_FIELD_NAME)) ? this[INTERNAL_ID_FIELD_NAME] as long? : null;

                return result.GetValueOrDefault();
            }
            set
            {
                throw new ArgumentNullException("value", "Internal identifier values are assigned by the Trackvia Service");
            }
        }

        /// <summary>
        /// This is used to handle comparing data returned from TrackVia when they give us
        /// string arrays for multiple choice options. By default comparing string arrays
        /// compare memory locations and we need to compare values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        /// <remarks>
        /// See SO Article: http://stackoverflow.com/questions/6196526/how-to-find-out-whether-two-string-arrays-are-equal-to-other?lq=1
        /// </remarks>
        static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        #region Implement Serializable

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
