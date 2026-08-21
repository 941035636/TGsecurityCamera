
    public delegate void Callback();
    public delegate string CallbackString<T3>(T3 a);

    public delegate void Callback<T>(T a);
    public delegate void Callback<T1, T2>(T1 a, T2 b);
    public delegate bool CallbackBool<T>(T a);

