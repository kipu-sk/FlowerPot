using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Task = System.Threading.Tasks.Task;

namespace FlowerPot
{
	[Command(PackageIds.MyCommand)]
	internal sealed class MyCommand : BaseCommand<MyCommand>
	{
		public static string RegistryKey {get;set;}	= "";

		static MyCommand()
		{
			RegistryKey	= $"Software\\{UserData.GlobalCompanyName}\\{UserData.GlobalProductName}";
		}


		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			UserData userData	= null;
			
			try
			{
				userData = UserData.FromRegistry(RegistryKey);
			}
			catch (Exception)
			{
				userData = new UserData();
			}
			
			DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();

			if (docView == null)
			{
				await VS.MessageBox.ShowWarningAsync("FlowerPot", $"docView == null");
			}

			string flowerPot = this.CreateFlowerPot(docView, userData);
				
			docView.TextBuffer.Insert(0, flowerPot);
		}

		private string CreateFlowerPot(DocumentView docView, UserData userData)
		{
			var selection			= docView.TextView.Selection;
			var currentSnapshot		= docView.TextBuffer.CurrentSnapshot;
			var currentLines		= currentSnapshot.Lines;

			List<string> lines		= new List<string>();

			foreach (var line in currentLines)
			{
				lines.Add(line.GetText());
			}

			string entity				= "Code file";
			string entityName			= "";
			string version				= "1.0";
			string fileName				= Path.GetFileName(docView.TextBuffer.GetFileName());
			
			foreach (string line in lines)
			{
				if (line.Contains("class "))
				{
					entity				= "Class";
					Regex rxClass		= new Regex("^.*class (?'Class'\\w*)");
					entityName			= rxClass.Match(line).Groups["Class"].Value;
					break;
				}
				else if (line.Contains("interface "))
				{
					entity				= "Interface";
					Regex rxInterface	= new Regex("^.*interface (?'Interface'\\w*)");
					entityName			= rxInterface.Match(line).Groups["Interface"].Value;
					break;
				}
				else if (line.Contains("enum "))
				{
					entity				= "Enum";
					Regex rxEnum		= new Regex("^.*enum (?'Enum'\\w*)");
					entityName			= rxEnum.Match(line).Groups["Enum"].Value;
					break;
				}
				else if (line.Contains("delegate "))
				{
					entity				= "Delegate";
					Regex rxDelegate	= new Regex("^.*delegate (?'Delegate'\\w*)");
					entityName			= rxDelegate.Match(line).Groups["Delegate"].Value;
					break;
				}
			}

			string[] fileHeader         = new string[8];
			
			fileHeader[0]				= $"/{new string('*', userData.HeaderWidth - 1)}";
			fileHeader[1]				= $"* File:         {fileName}";
			fileHeader[2]               = $"* Contents:     {entity} {entityName}";
			fileHeader[3]               = $"* Author:       {userData.AuthorName} ({userData.AuthorEmail})";
			fileHeader[4]				= $"* Date:         {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}";
			fileHeader[5]               = $"* Version:      {version}";
			fileHeader[6]               = $"* Copyright:    {userData.CompanyName} ({userData.CompanyWebsite})";
			fileHeader[7]				= $"{new string('*', userData.HeaderWidth - 1)}/";

			for (int i = 1; i <= 6; i++)
			{
				fileHeader[i]			+= new string(' ', userData.HeaderWidth - 1 - fileHeader[i].Length) + "*";
			}

			string result				= string.Join("\n", fileHeader) + "\n\n";

			return result;
		}
	}
}
