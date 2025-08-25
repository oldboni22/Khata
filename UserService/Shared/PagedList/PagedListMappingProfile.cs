using AutoMapper;

namespace Shared.PagedList;

public static class MappingProfileExtensions
{
    public static void AddPagedListMapping(this Profile profile)
    {
        profile.CreateMap(typeof(PagedList<>), typeof(PagedList<>))
            .ConvertUsing(typeof(PagedListConverter<,>));
    }
}