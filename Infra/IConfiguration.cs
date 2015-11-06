﻿using System;
namespace Infra {

    public interface IConfiguration {
        T Get<T>(string key, T defaultValue = default(T));
        TValue Get<TCaller, TValue>(string key, TValue defaultValue = default(TValue));
        T Get<T>(ConfigurationItem<T> src);
    }

}
