﻿using System;
using System.Linq;
using System.Reflection;

namespace Shared {
	public static class EnumExtensions {
		/// <summary>
		///     A generic extension method that aids in reflecting 
		///     and retrieving any attribute that is applied to an `Enum`.
		/// </summary>
		public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
			where TAttribute : Attribute
			=> enumValue.GetType()
				.GetMember(enumValue.ToString())
				.First()
				.GetCustomAttribute<TAttribute>();
	}
}