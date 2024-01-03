using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdemyAuthServer.Service
{
    public static class ObjectMapper
    {
        private static readonly Lazy<IMapper> lazy = new(() => {//ayaga kaldırınca memoryde olmayacak cagırınca oluşacak.
            var config = new MapperConfiguration(cfg =>//delegeler geriye bişey dönmeyen metot temsil eden yapılardır.
            {
                cfg.AddProfile<DtoMapper>();
            });

            return config.CreateMapper();
        });

        public static IMapper Mapper => lazy.Value;
    }
}
