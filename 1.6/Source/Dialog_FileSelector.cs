using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace MediaProps
{
	[HotSwappable]
	public class Dialog_FileSelector : Window
	{
		public MediaBuilding building;
		private string currentDirectoryPath;
		private List<string> drives;
		private Vector2 driveScrollPos;
		private Vector2 fileScrollPos;
		public Action onSelectAction;
		public Dialog_FileSelector(MediaBuilding building)
		{
			doCloseButton = true;
			doCloseX = true;
			forcePause = true;
			draggable = true;

			drives = new List<string>(Directory.GetLogicalDrives());
			this.building = building;
			currentDirectoryPath = drives[0];
		}

		public override Vector2 InitialSize => new Vector2(600f, 800f);

		public override void DoWindowContents(Rect inRect)
		{
			float maxDrivesPerRow = 5;
			float buttonHeight = 30f;
			float buttonSpacing = 5f;
			float drivePanelHeight = Mathf.Ceil(drives.Count / maxDrivesPerRow) * (buttonHeight + buttonSpacing);

			// Dynamically calculate path panel height based on the path text length
			float pathPanelWidth = inRect.width;
			float pathPanelHeight = Text.CalcHeight($"Current Path: {currentDirectoryPath}", pathPanelWidth) + 5f;

			float filePanelY = drivePanelHeight + pathPanelHeight + 20f;

			// Drives Panel (Top)
			Rect drivePanel = new Rect(inRect.x, inRect.y, inRect.width, drivePanelHeight);
			DrawDrivePanel(drivePanel, maxDrivesPerRow, buttonHeight, buttonSpacing);

			// Current Path Panel (Middle)
			Rect pathPanel = new Rect(inRect.x, inRect.y + drivePanelHeight + 10f, inRect.width, pathPanelHeight);
			DrawPathPanel(pathPanel);

			// File and Folder Panel (Bottom)
			Rect filePanel = new Rect(inRect.x, inRect.y + filePanelY, inRect.width, inRect.height - filePanelY - 50);
			DrawFilePanel(filePanel);
		}

		private void DrawDrivePanel(Rect rect, float maxDrivesPerRow, float buttonHeight, float buttonSpacing)
		{
			float buttonWidth = (rect.width - (maxDrivesPerRow - 1) * buttonSpacing) / maxDrivesPerRow;
			float totalHeight = Mathf.Ceil(drives.Count / maxDrivesPerRow) * (buttonHeight + buttonSpacing);

			Rect scrollRect = new Rect(0, 0, rect.width - 16f, totalHeight);
			Widgets.BeginScrollView(rect, ref driveScrollPos, scrollRect);

			for (int i = 0; i < drives.Count; i++)
			{
				int row = i / (int)maxDrivesPerRow;
				int col = i % (int)maxDrivesPerRow;
				float xPos = col * (buttonWidth + buttonSpacing);
				float yPos = row * (buttonHeight + buttonSpacing);

				Rect driveButtonRect = new Rect(xPos, yPos, buttonWidth, buttonHeight);
				if (Widgets.ButtonText(driveButtonRect, drives[i]))
				{
					currentDirectoryPath = drives[i];
				}
			}

			Widgets.EndScrollView();
		}

		private void DrawPathPanel(Rect rect)
		{
			Widgets.Label(rect, $"Current Path: {currentDirectoryPath}");
		}

		private void DrawFilePanel(Rect rect)
		{
			float buttonHeight = 30f;
			float yPosition = 0f;
			float extraPadding = 10f;

			// Filter for non-hidden directories and supported media files
			var directories = Directory.GetDirectories(currentDirectoryPath)
									   .Where(d => (new DirectoryInfo(d).Attributes & FileAttributes.Hidden) == 0)
									   .ToArray();

			var supportedFiles = Directory.GetFiles(currentDirectoryPath, "*.*")
										 .Where(file => building.AcceptableExtensions.Contains(Path.GetExtension(file).ToLower()))
										 .Where(f => (new FileInfo(f).Attributes & FileAttributes.Hidden) == 0)
										 .ToArray();

			// Calculate the required height for scrolling based on the count of non-hidden directories and supported files,
			// adding the height of the ".. (Up)" button if applicable
			float totalHeight = (directories.Length + supportedFiles.Length) * (buttonHeight + 5f) + extraPadding;
			if (Directory.GetParent(currentDirectoryPath) != null)
			{
				totalHeight += buttonHeight + 5f; // Add height for the ".. (Up)" button
			}

			Rect scrollRect = new Rect(0, 0, rect.width - 16f, totalHeight);
			Widgets.BeginScrollView(rect, ref fileScrollPos, scrollRect);

			// Align text to the middle left
			Text.Anchor = TextAnchor.MiddleLeft;

			// Up Navigation Button
			if (Directory.GetParent(currentDirectoryPath) != null)
			{
				Rect upButtonRect = new Rect(0, yPosition, rect.width - 16f, buttonHeight);
				if (Widgets.ButtonText(upButtonRect, ".. (Up)"))
				{
					currentDirectoryPath = Directory.GetParent(currentDirectoryPath).FullName;
				}
				yPosition += buttonHeight + 5f;
			}

			// Display Non-Hidden Subdirectories
			foreach (var directory in directories)
			{
				Rect dirButtonRect = new Rect(0, yPosition, rect.width - 16f, buttonHeight);
				if (Widgets.ButtonText(dirButtonRect, Path.GetFileName(directory) + "/"))
				{
					currentDirectoryPath = directory;
				}
				yPosition += buttonHeight + 5f;
			}

			// Display Supported Non-Hidden Media Files
			foreach (var filePath in supportedFiles)
			{
				Rect fileButtonRect = new Rect(0, yPosition, rect.width - 16f, buttonHeight);
				if (Widgets.ButtonText(fileButtonRect, Path.GetFileName(filePath)))
				{
					building.selectedFilePath = filePath;
					onSelectAction();
					Close();
				}
				yPosition += buttonHeight + 5f;
			}

			// Reset text alignment after finishing
			Text.Anchor = TextAnchor.UpperLeft;

			Widgets.EndScrollView();
		}
	}
}
