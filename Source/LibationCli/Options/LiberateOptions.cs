﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using DataLayer;
using FileLiberator;

namespace LibationCli
{
	[Verb("liberate", HelpText = "Liberate: book and pdf backups. Default: download and decrypt all un-liberated titles and download pdfs. "
		+ "Optional: use 'pdf' flag to only download pdfs.")]
	public class LiberateOptions : ProcessableOptionsBase
	{
		[Option(shortName: 'p', longName: "pdf", Required = false, Default = false, HelpText = "Flag to only download pdfs")]
		public bool PdfOnly { get; set; }

		protected override Task ProcessAsync()
			=> PdfOnly
			? RunAsync(CreateProcessable<DownloadPdf>())
			: RunAsync(CreateBackupBook());

		private static Processable CreateBackupBook()
		{
			var downloadPdf = CreateProcessable<DownloadPdf>();

			//Chain pdf download on DownloadDecryptBook.Completed
			void onDownloadDecryptBookCompleted(object sender, LibraryBook e)
			{
				// this is fast anyway. run as sync for easy exception catching
				downloadPdf.TryProcessAsync(e).GetAwaiter().GetResult();
			}

			var downloadDecryptBook = CreateProcessable<DownloadDecryptBook>(onDownloadDecryptBookCompleted);
			return downloadDecryptBook;
		}
	}
}
