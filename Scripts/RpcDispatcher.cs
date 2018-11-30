using System;
using System.Collections.Generic;


namespace UniJSON
{
    public class RpcDispatcher
    {
        delegate void Callback(IValueNode args, IFormatter f);
        Dictionary<string, Callback> m_map = new Dictionary<string, Callback>();

        #region Action
        public void Register<A0>(string method, Action<A0> action)
        {
            m_map.Add(method, (args, f) =>
            {
                var it = args.ArrayItems.GetEnumerator();

                var a0 = default(A0);
                it.MoveNext();
                it.Current.Deserialize(ref a0);

                action(a0);
            });
        }

        public void Register<A0, A1>(string method, Action<A0, A1> action)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Func
        public void Register<A0, A1, R>(string method, Func<A0, A1, R> action)
        {
            m_map.Add(method, (args, f) =>
            {
                var it = args.ArrayItems.GetEnumerator();

                var a0 = default(A0);
                it.MoveNext();
                it.Current.Deserialize(ref a0);

                var a1 = default(A1);
                it.MoveNext();
                it.Current.Deserialize(ref a1);

                var r = action(a0, a1);
                f.Serialize(r);
            });
        }
        #endregion

        public void Call(string method, IValueNode args, IFormatter f = null)
        {
            Callback callback;
            if (!m_map.TryGetValue(method, out callback))
            {
                throw new KeyNotFoundException();
            }
            callback(args, f);
        }
    }
}
