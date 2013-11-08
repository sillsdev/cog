using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Ionic.Zip;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.Import
{
	public class KmlGeographicRegionsImporter : IGeographicRegionsImporter
	{
		private const string DefaultNamespace = "http://www.opengis.net/kml/2.2";
		private static readonly XNamespace Kml = DefaultNamespace;

		public object CreateImportSettingsViewModel()
		{
			return null;
		}

		public void Import(object importSettingsViewModel, Stream stream, CogProject project)
		{
			XDocument doc;
			if (ZipFile.IsZipFile(stream, false))
			{
				stream.Seek(0, SeekOrigin.Begin);
				ZipFile zipFile = ZipFile.Read(stream);
				ZipEntry kmlEntry = zipFile.First(entry => entry.FileName.EndsWith(".kml"));
				doc = XDocument.Load(kmlEntry.OpenReader(), LoadOptions.SetLineInfo);
			}
			else
			{
				doc = XDocument.Load(stream, LoadOptions.SetLineInfo);
			}
			XElement root = doc.Root;
			Debug.Assert(root != null);

			if (root.GetDefaultNamespace() != DefaultNamespace)
				throw new ImportException("The specified file is not a KML file.");

			XElement document = root.Element(Kml + "Document");
			if (document == null)
				throw new ImportException("Missing Document element.");

			var regions = new Dictionary<Variety, List<GeographicRegion>>();
			LoadFolder(project, regions, document);

			foreach (KeyValuePair<Variety, List<GeographicRegion>> varietyRegions in regions)
				varietyRegions.Key.Regions.AddRange(varietyRegions.Value);
		}

		private void LoadFolder(CogProject project, Dictionary<Variety, List<GeographicRegion>> regions, XElement elem)
		{
			foreach (XElement placemark in elem.Elements(Kml + "Placemark"))
			{
				var name = (string) placemark.Element(Kml + "name");
				Variety variety;
				if (!string.IsNullOrEmpty(name) && project.Varieties.TryGetValue(name, out variety))
				{
					XElement polygon = placemark.Element(Kml + "Polygon");
					if (polygon != null)
						regions.GetValue(variety, () => new List<GeographicRegion>()).Add(LoadRegion(polygon, (string) placemark.Element(Kml + "description")));
				}
			}

			foreach (XElement folder in elem.Elements(Kml + "Folder"))
			{
				var name = (string) folder.Element(Kml + "name");
				Variety variety;
				if (!string.IsNullOrEmpty(name) && project.Varieties.TryGetValue(name, out variety))
					LoadVarietyFolder(regions, variety, folder);
				else
					LoadFolder(project, regions, folder);
			}
		}

		private void LoadVarietyFolder(Dictionary<Variety, List<GeographicRegion>> regions, Variety variety, XElement elem)
		{
			foreach (XElement placemark in elem.Elements(Kml + "Placemark"))
			{
				XElement polygon = placemark.Element(Kml + "Polygon");
				if (polygon != null)
					regions.GetValue(variety, () => new List<GeographicRegion>()).Add(LoadRegion(polygon, (string) placemark.Element(Kml + "name")));
			}

			foreach (XElement folder in elem.Elements(Kml + "Folder"))
				LoadVarietyFolder(regions, variety, folder);
		}

		private GeographicRegion LoadRegion(XElement polygon, string desc)
		{
			XElement coords = polygon.Elements(Kml + "outerBoundaryIs").Elements(Kml + "LinearRing").Elements(Kml + "coordinates").FirstOrDefault();
			if (coords == null || string.IsNullOrEmpty((string) coords))
				throw new ImportException(string.Format("A Polygon element does not contain coordinates. Line: {0}", ((IXmlLineInfo) polygon).LineNumber));

			var region = new GeographicRegion {Description = desc};
			string[] coordsArray = ((string) coords).Split().Where(coord => !string.IsNullOrEmpty(coord)).ToArray();
			for (int i = 0; i < coordsArray.Length - 1; i++)
			{
				string[] coordArray = coordsArray[i].Split(',');
				region.Coordinates.Add(new GeographicCoordinate(double.Parse(coordArray[1]), double.Parse(coordArray[0])));
			}
			return region;
		}
	}
}
