namespace UniJSON
{
    public interface IRpc
    {
        void Request(string method);
        void Request<A0>(string method, A0 a0);
        void Request<A0, A1>(string method, A0 a0, A1 a1);
        void Request<A0, A1, A2>(string method, A0 a0, A1 a1, A2 a2);
        void Request<A0, A1, A2, A3>(string method, A0 a0, A1 a1, A2 a2, A3 a3);
        void Request<A0, A1, A2, A3, A4>(string method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);
        void Request<A0, A1, A2, A3, A4, A5>(string method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
        void ResponseSuccess(int id);
        void ResponseSuccess<T>(int id, T result);
        void ResponseError(int id, System.Exception error);
        void Notify(string method);
        void Notify<A0>(string method, A0 a0);
        void Notify<A0, A1>(string method, A0 a0, A1 a1);
        void Notify<A0, A1, A2>(string method, A0 a0, A1 a1, A2 a2);
        void Notify<A0, A1, A2, A3>(string method, A0 a0, A1 a1, A2 a2, A3 a3);
        void Notify<A0, A1, A2, A3, A4>(string method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);
        void Notify<A0, A1, A2, A3, A4, A5>(string method, A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
    }
}
