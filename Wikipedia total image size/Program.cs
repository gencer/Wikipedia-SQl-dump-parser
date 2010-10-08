﻿using System;
using System.Collections.Generic;
using System.Linq;
using WpSqlDumpParser;
using WpSqlDumpParser.EntityCollections;

namespace WpTotalImageSize
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Wiki [{0}]: ", Settings.Default.Wiki);
			string wiki = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(wiki))
				wiki = Settings.Default.Wiki;
			Settings.Default.Wiki = wiki;
			Console.Write("Date [{0}]: ", Settings.Default.Date);
			string dateString = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(dateString))
				dateString = Settings.Default.Date;
			if (string.IsNullOrWhiteSpace(dateString))
				dateString = DateTime.Today.ToString("yyyyMMdd");
			Settings.Default.Date = dateString;
			Console.Write("Commons date [{0}]: ", Settings.Default.CommonsDate);
			string commonsDateString = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(commonsDateString))
				commonsDateString = Settings.Default.CommonsDate;
			if (string.IsNullOrWhiteSpace(commonsDateString))
				commonsDateString = DateTime.Today.ToString("yyyyMMdd");
			Settings.Default.CommonsDate = commonsDateString;
			Settings.Default.Save();

			DateTime date = DateTime.ParseExact(dateString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
			DateTime commonsDate = DateTime.ParseExact(commonsDateString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

			var imageLinks = ImageLinks.Instance.Get(wiki, date, false);
			SortedSet<string> linkNames = new SortedSet<string>(imageLinks.Select(link => link.ToTitle));

			var mergedImages = OrderedSetOperations.Merge(new[]
			{
				Images.Instance.Get(wiki, date),
				Images.Instance.Get("commonswiki", commonsDate)
			});

			var imagesEnumerator = mergedImages.GetEnumerator();
			var linksEnumerator = linkNames.GetEnumerator();
			bool finished = !imagesEnumerator.MoveNext() || !linksEnumerator.MoveNext();

			long totalSize = 0;
			int totalCount = 0;
			int missesCount = 0;

			while (!finished)
			{
				int comparison = linksEnumerator.Current.CompareTo(imagesEnumerator.Current.Name);
				if (comparison > 0)
					finished = !imagesEnumerator.MoveNext();
				else if (comparison < 0)
				{
					missesCount++;
					finished = !linksEnumerator.MoveNext();
				}
				else
				{
					totalSize += imagesEnumerator.Current.Size;
					totalCount++;
					finished = !imagesEnumerator.MoveNext() || !linksEnumerator.MoveNext();
				}
			}

			Console.WriteLine("{0} different images is used, totalling {1:f2} MB. {2} files weren't accounted for.", totalCount, totalSize, missesCount);
		}
	}
}