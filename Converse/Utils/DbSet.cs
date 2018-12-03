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
		public static IEnumerable<T> FindPredicate<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate)
			where T : class
		{
			var local = dbSet.Local.Where(predicate.Compile());
			var findPredicate = local as T[] ?? local.ToArray();
			return findPredicate.Any() ? findPredicate : dbSet.Where(predicate).ToArray();
		}

		public static T FirstPredicate<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate)
			where T : class
		{
			var local = dbSet.Local.FirstOrDefault(predicate.Compile());
			return local ?? dbSet.FirstOrDefault(predicate);
		}
	}
}
