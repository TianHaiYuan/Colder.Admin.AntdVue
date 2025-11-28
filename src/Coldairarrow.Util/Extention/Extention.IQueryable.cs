using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Coldairarrow.Util.PageInput;

namespace Coldairarrow.Util
{
    /// <summary>
    /// IQueryable"T"的拓展操作
    /// </summary>
    public static partial class Extention
    {
        /// <summary>
        /// 符合条件则Where
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="q">数据源</param>
        /// <param name="need">是否符合条件</param>
        /// <param name="where">筛选</param>
        /// <returns></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> q, bool need, Expression<Func<T, bool>> where)
        {
            if (need)
            {
                return q.Where(where);
            }
            else
            {
                return q;
            }
        }

        /// <summary>
        /// 动态排序法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">IQueryable数据源</param>
        /// <param name="sortColumn">排序的列</param>
        /// <param name="sortType">排序的方法</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string sortColumn, string sortType)
        {
            //return source.OrderBy(new KeyValuePair<string, string>(sortColumn, sortType));
            return source.OrderBy($"{sortColumn} {sortType}");
        }

        /// <summary>
        /// 动态排序法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="sort">排序规则，Key为排序列，Value为排序类型</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, params KeyValuePair<string, string>[] sort)
        {
            var parameter = Expression.Parameter(typeof(T), "o");

            sort.ForEach((aSort, index) =>
            {
                //根据属性名获取属性
                var property = GetTheProperty(typeof(T), aSort.Key);
                //创建一个访问属性的表达式
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);

                string OrderName = "";
                if (index > 0)
                {
                    OrderName = aSort.Value.ToLower() == "desc" ? "ThenByDescending" : "ThenBy";
                }
                else
                    OrderName = aSort.Value.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";

                MethodCallExpression resultExp = Expression.Call(
                    typeof(Queryable), OrderName,
                    new Type[] { typeof(T), property.PropertyType },
                    source.Expression,
                    Expression.Quote(orderByExp));

                source = source.Provider.CreateQuery<T>(resultExp);
            });

            return (IOrderedQueryable<T>)source;

            //必须追溯到最基类属性
            PropertyInfo GetTheProperty(Type type, string propertyName)
            {
                if (type.BaseType.GetProperties().Any(x => x.Name == propertyName))
                    return GetTheProperty(type.BaseType, propertyName);
                else
                    return type.GetProperty(propertyName);
            }
        }

        /// <summary>
        /// 动态排序法（支持Sort集合）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="sorts">排序集合</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<Sort> sorts)
        {
            if (sorts == null || !sorts.Any())
            {
                throw new ArgumentException("排序参数sorts不能为空");
            }

            var sortList = sorts.ToList();
            var orderByString = string.Join(", ", sortList.Select(s => $"{s.Field} {s.Type}"));
            return source.OrderBy(orderByString);
        }

        /// <summary>
        /// 获取分页数据(仅获取列表,不获取总数量)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="pageInput">分页参数</param>
        /// <returns></returns>
        public static async Task<List<T>> GetPageListAsync<T>(this IQueryable<T> source, PageInput pageInput)
        {
            var list = await source.OrderBy(pageInput.Sorts)
                .Skip((pageInput.PageIndex - 1) * pageInput.PageRows)
                .Take(pageInput.PageRows)
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// 获取分页数据同步方法
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <param name="pageInput"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static PageResult<TEntity> GetPageResult<TEntity>(this IQueryable<TEntity> entities, PageInput pageInput) where TEntity : new()
        {
            if (pageInput.PageIndex <= 0)
            {
                throw new InvalidOperationException("pageIndex must be a positive integer greater than 0.");
            }
            int num = entities.Count();

            if (!pageInput.Sorts.IsNullOrEmpty())
            {
                entities = entities.OrderBy(pageInput.Sorts);
            }
            List<TEntity> items = entities
                .Skip((pageInput.PageIndex - 1) * pageInput.PageRows)
                .Take(pageInput.PageRows)
                .ToList();
            int num2 = (int)Math.Ceiling(num / (double)pageInput.PageRows);
            return new PageResult<TEntity>
            {
                Data = items,
                TotalCount = num,
                TotalPages = num2,
                HasNextPages = (pageInput.PageIndex < num2),
                HasPrevPages = (pageInput.PageIndex - 1 > 0)
            };
        }

        /// <summary>
        /// 获取分页数据异步方法
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <param name="pageInput"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<PageResult<TEntity>> GetPageResultAsync<TEntity>(this IQueryable<TEntity> entities, PageInput pageInput) where TEntity : new()
        {
            if (pageInput.PageIndex <= 0)
            {
                throw new InvalidOperationException("pageInput.PageIndex must be a positive integer greater than 0.");
            }
            int totalCount = await entities.CountAsync();

            if (!pageInput.Sorts.IsNullOrEmpty())
            {
                entities = entities.OrderBy(pageInput.Sorts);
            }
            List<TEntity> items = await entities
                .Skip((pageInput.PageIndex - 1) * pageInput.PageRows)
                .Take(pageInput.PageRows)
                .ToListAsync();
            int num = (int)Math.Ceiling(totalCount / (double)pageInput.PageRows);
            return new PageResult<TEntity>
            {
                Data = items,
                TotalCount = totalCount,
                TotalPages = num,
                HasNextPages = (pageInput.PageIndex < num),
                HasPrevPages = (pageInput.PageIndex - 1 > 0)
            };
        }
    }
}
