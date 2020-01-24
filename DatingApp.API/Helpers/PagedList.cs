using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers
{
  public class PagedList<T> : List<T> where T : class
  {
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; } // of items

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
      TotalCount = count;
      PageSize = pageSize;
      CurrentPage = pageNumber;
      // 13 / 5 = 3
      TotalPages = (int)Math.Ceiling(count / (double)pageSize);
      this.AddRange(items); // add the users to the actual paged list
    }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
      // 13 users and 5 users per page
      // pageSize = 5
      var count = await source.CountAsync(); // 13
      // PageNumber = 2 (skip (2 - 1) * 5) - skips the first five elements and returns the next 5
      var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
      return new PagedList<T>(items, count, pageNumber, pageSize);
    }
  }
}