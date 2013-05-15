using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Ionic.Zip;

namespace SIL.Cog.Import
{
	public class KmlGeographicRegionsImporter : IGeographicRegionsImporter
	{
		private class ResourceXmlResolver : XmlResolver
		{
			public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
			{
				return GetType().Assembly.GetManifestResourceStream(string.Format("SIL.Cog.Import.{0}", Path.GetFileName(absoluteUri.ToString())));
			}

			public override ICredentials Credentials
			{
				set { throw new NotImplementedException(); }
			}
		}

		private const string DefaultNamespace = "http://www.opengis.net/kml/2.2";
		private static readonly XNamespace Kml = DefaultNamespace;
		private static readonly XmlSchemaSet Schema;

		static KmlGeographicRegionsImporter()
		{
			Schema = new XmlSchemaSet {XmlResolver = new ResourceXmlResolver()};
			Schema.Add(DefaultNamespace, "ogckml22.xsd");
			Schema.Compile();
		}

		public void Import(string path, CogProject project)
		{
			XDocument doc;
			if (ZipFile.IsZipFile(path))
			{
				ZipFile zipFile = ZipFile.Read(path);
				ZipEntry kmlEntry = zipFile.First(entry => entry.FileName.EndsWith(".kml"));
				doc = XDocument.Load(kmlEntry.OpenReader());
			}
			else
			{
				doc = XDocument.Load(path);
			}
			XElement root = doc.Root;
			Debug.Assert(root != null);

			if (root.GetDefaultNamespace() != DefaultNamespace)
				throw new ArgumentException("The specified file is not a valid KML file", "path");

			doc.Validate(Schema, null);

			XElement document = root.Element(Kml + "Document");
			LoadFolder(project.Varieties.ToDictionary(v => v.Name.ToLowerInvariant()), document);
		}

		private void LoadFolder(Dictionary<string, Variety> varieties, XElement elem)
		{
			foreach (XElement placemark in elem.Elements(Kml + "Placemark"))
			{
				var name = (string) placemark.Element(Kml + "name");
				Variety variety;
				if (!string.IsNullOrEmpty(name) && varieties.TryGetValue(name.ToLowerInvariant(), out variety))
				{
					XElement polygon = placemark.Element(Kml + "Polygon");
					if (polygon != null)
						LoadRegion(variety, polygon, (string) placemark.Element(Kml + "description"));
				}
			}

			foreach (XElement folder in elem.Elements(Kml + "Folder"))
			{
				var name = (string) folder.Element(Kml + "name");
				Variety variety;
				if (!string.IsNullOrEmpty(name) && varieties.TryGetValue(name.ToLowerInvariant(), out variety))
				{
					LoadVarietyFolder(variety, folder);
				}
				else
				{
					LoadFolder(varieties, folder);
				}
			}
		}

		private void LoadVarietyFolder(Variety variety, XElement elem)
		{
			foreach (XElement placemark in elem.Elements(Kml + "Placemark"))
			{
				XElement polygon = placemark.Element(Kml + "Polygon");
				if (polygon != null)
					LoadRegion(variety, polygon, (string) placemark.Element(Kml + "name"));
			}

			foreach (XElement folder in elem.Elements(Kml + "Folder"))
				LoadVarietyFolder(variety, folder);
		}

		private void LoadRegion(Variety variety, XElement polygon, string desc)
		{
			XElement coords = polygon.Elements(Kml + "outerBoundaryIs").Elements(Kml + "LinearRing").Elements(Kml + "coordinates").First();
			var region = new GeographicRegion {Description = desc};
			string[] coordsArray = ((string) coords).Split().Where(coord => !string.IsNullOrEmpty(coord)).ToArray();
			for (int i = 0; i < coordsArray.Length - 1; i++)
			{
				string[] coordArray = coordsArray[i].Split(',');
				region.Coordinates.Add(new GeographicCoordinate(double.Parse(coordArray[1]), double.Parse(coordArray[0])));
			}
			variety.Regions.Add(region);
		}
	}
}
