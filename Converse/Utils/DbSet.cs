using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Converse.Utils
{
	public static class DbSet
	{
		//private static IEnumerable<T> FilterDeleted<T>(DbContext dbContext, IEnumerable<T> data)
		//	where T : class
		//{
		//	return data.Where(d => dbContext.Entry(d).State != EntityState.Deleted);
		//}
		
		//public static IEnumerable<T> WherePredicate<T>(this DbSet<T> dbSet, DbContext dbContext, Expression<Func<T, bool>> predicate)
		//	where T : class
		//{
		//	var compiledPredicate = predicate.Compile();
		//	var local = FilterDeleted(dbContext, dbSet.Local).Where(compiledPredicate);
		//	var findPredicate = local as T[] ?? local.ToArray();
		//	return findPredicate.Any() ? findPredicate : FilterDeleted(dbContext, dbSet).Where(compiledPredicate).ToArray();
		//}

		//public static T FirstPredicate<T>(this DbSet<T> dbSet, DbContext dbContext, Expression<Func<T, bool>> predicate)
		//	where T : class
		//{
		//	var compiledPredicate = predicate.Compile();
		//	var local = FilterDeleted(dbContext, dbSet.Local).FirstOrDefault(compiledPredicate);
		//	return local ?? FilterDeleted(dbContext, dbSet).FirstOrDefault(compiledPredicate);
		//}
	}
}
