using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentCrm.Application.GlobalAppException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Profiles
{
    public class GlobalMappingProfile : Profile
    {
        public GlobalMappingProfile()
        {
            CreateMap<string, Guid>().ConvertUsing<StringToGuidConverter>();
            CreateMap<Guid, string>().ConvertUsing<GuidToStringConverter>();

            // Nullable support (optional)
            CreateMap<string, Guid?>().ConvertUsing((src, dest, ctx) =>
                string.IsNullOrWhiteSpace(src) ? (Guid?)null :
                Guid.TryParse(src, out var g) ? g :
                throw new StudentCrm.Application.GlobalAppException.GlobalAppException($"Yanlış Guid formatı: {src}")
            );

            CreateMap<Guid?, string>().ConvertUsing((src, dest, ctx) =>
                src.HasValue ? src.Value.ToString() : null!
            );
        }
    }
}
public class StringToGuidConverter : ITypeConverter<string, Guid>
{
    public Guid Convert(string source, Guid destination, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new GlobalAppException("Guid boş ola bilməz!");

        if (!Guid.TryParse(source, out var guid))
            throw new GlobalAppException($"Yanlış Guid formatı: {source}");

        return guid;
    }
}

public class GuidToStringConverter : ITypeConverter<Guid, string>
{
    public string Convert(Guid source, string destination, ResolutionContext context)
        => source.ToString();
}