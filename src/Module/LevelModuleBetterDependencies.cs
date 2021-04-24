using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ThunderRoad;
using UnityEngine;
using Wully.Extensions;
using Wully.Helpers;
using static Wully.Helpers.BetterLogger;

namespace Wully.Module {
	public class LevelModuleBetterDependencies : LevelModule {

		private static BetterLogger log = BetterLogger.GetLogger(typeof(LevelModuleBetterDependencies));

		// Configurables
		/// <summary>
		/// Enable/Disable Better Logging for BetterEvents
		/// </summary>
		public bool enableLogging = true;
		/// <summary>
		/// Set the GetLogLevel for BetterEvents
		/// </summary>
		public LogLevel logLevel = LogLevel.Info;

		public LevelModuleBetterDependencies local;

		private String gameVersion;
		private String minModVersion;
		/// <summary>
		/// Called when a level is loaded
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public override IEnumerator OnLoadCoroutine( Level level ) {
			try {
				log.SetLoggingLevel(logLevel);
				log.DisableLogging();
				if ( enableLogging ) {
					log.EnableLogging();
				}
				log.Debug().Message("level {0}", level.data.id);

				
				//master scene is always loaded and this module gets loaded with it
				if ( level.data.id.ToLower() == "master" ) {


					log.Info().Message($"Initialized Wully's BetterMods - BetterDependencies Module".Italics());
					
					local = this;
					
					gameVersion = GameManager.local.build;
					minModVersion = GameManager.local.minModVersion;
					log.Info().Message($"GameVersion: {gameVersion}. Minimum Mod Version {minModVersion}");

					CheckDependencies();

				}

			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}

			yield break;
		}

		

		//Foldername -> manifest.json
		private Dictionary<string, ModManifest> manifests;
		//Foldername -> dependencies.json
		private Dictionary<string, List<ModDependency>> dependencies;


		private void ManifestValidation() {
			//Find all the VALID, ENABLED, mods manifest.json files and parse them,
			manifests = ReadManifests();
			log.Info().Message("Manifest Validation..");
			foreach ( KeyValuePair<string, ModManifest> keyValuePair in manifests ) {
				string mod = keyValuePair.Key;
				ModManifest manifest = keyValuePair.Value;
				//Check if the mods version matches the game version.
				if ( !manifest.GameVersion.Equals(gameVersion) ) {
					log.Warn().Message($"The mod: {mod}'s version {manifest.GameVersion} does not match the current game version: {gameVersion}. You may need to update {mod}.");
				}
				if ( !manifest.Name.Equals(mod) ) {
					log.Warn().Message($"The mod: {mod}'s folder name {mod} does not match the manifest name: {manifest.Name}. Vortex installations may not work. Please alert {manifest.Author} to fix it.");
				}

				//check if the manifest exists twice 
				foreach ( KeyValuePair<string, ModManifest> keyValuePair2 in manifests ) {
					// if the folder name is different
					if ( !mod.Equals(keyValuePair2.Key) ) {
						//but the manifest mod name is the same, possible duplicate mod
						if ( manifest.Name.Equals(keyValuePair2.Value.Name) ) {
							log.Warn().Message($"Possible duplicate mod manifest found - folder {mod} and folder {keyValuePair2.Key} contain manifest with the same mod name - {manifest.Name}. Please check your mods folder for duplicate mods.");
						}
					}
				}
			}

			log.Info().Message("DLL Dependency Validation..");
			foreach (string mod in manifests.Keys) {
				if (modAssemblyDependencies.TryGetValue(mod, out List<string> assm)) {
					log.Debug().Message($"Found mod: {mod} with assembly dependencies: {string.Join(", ", assm)}");
					//check if the assm dependencies exist
					foreach ( string assemblyName in assm) {

						if (!loadedAssemblies.Contains(assemblyName)) {
							log.Warn().Message(
								$"Mod: {mod} Requires DLL - {assemblyName}, but it could not be found");
						}
					}
				}
			}


		}

		private void DependencyCheck() {
			dependencies = ReadDependencies();
			log.Info().Message("Checking Dependencies..");



			foreach ( KeyValuePair<string, List<ModDependency>> keyValuePair in dependencies ) {
				string mod = keyValuePair.Key;
				List<ModDependency> depList = keyValuePair.Value;
				log.Debug().Message($"Found mod: {mod} with json specified dependencies: {string.Join(", ", depList)}");

				foreach ( ModDependency modDependency in depList ) {
					bool found = false;
					//First check based on the mod folder name if the mod dependency exists
					if ( manifests.TryGetValue(modDependency.Name, out ModManifest manifest) ) {

						//check version 
						if ( !manifest.ModVersion.Equals(modDependency.Version) ) {
							log.Warn().Message($"Mod: {mod} Requires - {modDependency.Name}. Installed version of {modDependency.Name} - {modDependency.Version} does not match required version {manifest.ModVersion}. Please update the mod if you have a older version.");
						} else {
							log.Info().Message($"Mod: {mod} Requires - {modDependency.Name}-{modDependency.Version}. Dependency mod - {manifest.Name}-{manifest.ModVersion} exists.");
						}
						continue;
					}

					//next check based on the installed mods manifest names - incase the folder name != the manifest.json mod name
					foreach ( ModManifest modManifest in manifests.Values ) {

						if ( modManifest.Name.Equals(modDependency.Name) ) {
							//check version 
							found = true;
							if ( !modManifest.ModVersion.Equals(modDependency.Version) ) {
								log.Warn().Message($"Mod: {mod} Requires - {modDependency.Name}. Installed version of {modDependency.Name} - {modDependency.Version} does not match required version {modManifest.ModVersion}. Please update the mod if you have a older version.");
							} else {
								log.Warn().Message($"Mod: {mod} Requires - {modDependency.Name}. Dependency mod folder name {mod} does not match dependency mods manifest name : {modManifest.Name}. Please alert {modManifest.Author} to fix it.");
								log.Info().Message($"Mod: {mod} Requires - {modDependency.Name}-{modDependency.Version}. Dependency mod {modManifest.Name}-{modManifest.ModVersion} exists.");
							}
						}
					}

					if ( !found ) {
						log.Error().Message($"Mod: {mod} Requires - {modDependency.Name}. Mod does not appear to be installed. Please check you have downloaded the required mod");
					}
				}
			}
		}

		private HashSet<string> loadedAssemblies = new HashSet<string>();
		//mod name -> list of the assemblies it depends on
		private Dictionary<string, List<string>> modAssemblyDependencies = new Dictionary<string, List<string>>();
		public void CheckDependencies() {

			log.Debug().Message($"BaseDir: {AppDomain.CurrentDomain.BaseDirectory}");
			//https://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity/36244111#36244111

			//get all the already loaded assemblies from the appdomain.
			loadedAssemblies.UnionWith(GetAssemblies());

			// check loaded assemblies for mod dll if it has one, then check if that dll needs a referenced dll
			foreach ( string text in Directory.GetDirectories(FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, "")) ) {
				if ( !Path.GetFileName(text).StartsWith("_") && File.Exists(text + "/manifest.json") ) {
					foreach ( FileInfo fileInfo in new DirectoryInfo(text + "/").GetFiles("*.dll", SearchOption.AllDirectories) ) {
						string folderName = new DirectoryInfo(text).Name;

						Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
						log.Debug().Message(assembly.ToString());
						//Add the assembly to the list of loaded assemblies
						loadedAssemblies.Add(assembly.GetName().Name);

						//get the mod dll's referenced assemblies and add them to the dependency list

						foreach ( AssemblyName an in assembly.GetReferencedAssemblies() ) {
							log.Debug().Message("Name={0}, Version={1}, Culture={2}, PublicKey token={3}", an.Name, an.Version, an.CultureInfo.Name, (BitConverter.ToString(an.GetPublicKeyToken())));
							if ( modAssemblyDependencies.TryGetValue(folderName, out List<string> listAssemblies) ) {
								listAssemblies.Add(an.Name);
							} else {
								List<string> list = new List<string>();
								list.Add(an.Name);
								modAssemblyDependencies.Add(folderName, list);
							}
						}
					}
				}
			}

			log.Debug().Message($"loaded Assemblies: { string.Join(", ", loadedAssemblies)}");
			string modDeps = "mod Assembly Dependencies:";
			foreach ( KeyValuePair<string, List<string>> modAssemblyDependency in modAssemblyDependencies ) {
				modDeps += modAssemblyDependency.Key + string.Join(", ", modAssemblyDependency.Value);
			}
			log.Debug().Message($"{modDeps}");

			ManifestValidation();
			DependencyCheck();
			
		}

		private List<string> GetAssemblies() {
			var returnAssemblies = new List<string>();

			AppDomain currentDomain = AppDomain.CurrentDomain;
			//Provide the current application domain evidence for the assembly.
			Evidence asEvidence = currentDomain.Evidence;
			
			//Make an array for the list of assemblies.
			Assembly[] assems = currentDomain.GetAssemblies();

			//List the assemblies in the current application domain.

			foreach (Assembly assem in assems) {
				
				loadedAssemblies.Add(assem.GetName().Name);
			}
			var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/BladeAndSorcery_Data/Managed/", "*.dll")
				.Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)));

			foreach ( Assembly assem in assemblies ) {

				loadedAssemblies.Add(assem.GetName().Name);
			}

			return returnAssemblies;
		}

		private List<string> GetValidModDirs() {
			List<string> modPaths = new List<string>();

			//Search mod directory
			foreach ( string text in Directory.GetDirectories(FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, "")) ) {
				//Find enabled mods that have a manifest.json
				if ( !Path.GetFileName(text).StartsWith("_") && File.Exists(text + "/manifest.json") ) {
					//add it to our list
					modPaths.Add(text);
				}
			}

			return modPaths;
		}

		private Dictionary<string,ModManifest> ReadManifests() {
			Dictionary<string, ModManifest> manifests = new Dictionary<string, ModManifest>();

			foreach (string dir in GetValidModDirs() ) {
				//get the mod folder name
				log.Debug().Message($"Trying to read manifest for {dir}");
				string folderName = new DirectoryInfo(dir).Name;
				string manifest = File.ReadAllText(dir + "/manifest.json");
				if (manifest != null && manifest.Length > 0) {
					log.Debug().Message($"manifest : {manifest.EscapeJsonBraces()}");
					ModManifest modManifest = JsonUtility.FromJson<ModManifest>(manifest);
					manifests.Add(folderName, modManifest);
				}
			}

			return manifests;
		}

		private Dictionary<string, List<ModDependency>> ReadDependencies() {
			Dictionary<string, List<ModDependency>> modDependencies = new Dictionary<string, List<ModDependency>>();

			foreach ( string dir in GetValidModDirs() ) {
				//get the mod folder name
				log.Debug().Message($"Trying to read dependencies for {dir}");
				string folderName = new DirectoryInfo(dir).Name;
				string filePath = dir + "/dependencies.json";

				if ( File.Exists(filePath) ){
					string dependencies = File.ReadAllText(dir + "/dependencies.json");
					if (dependencies != null && dependencies.Length > 0) {
						log.Debug().Message($"dependencies : {dependencies.EscapeJsonBraces()}");
						List<ModDependency> modDependency = JsonConvert.DeserializeObject<List<ModDependency>>(dependencies);
						foreach (var dependency in modDependency ) {
							log.Debug().Message($"modDependency : {dependency}");
						}
							
						
						modDependencies.Add(folderName, modDependency);
					}
				} else {
					log.Debug().Message($"No Dependency file for {folderName}");
				}
			}

			return modDependencies;
		}
	}

	[Serializable]
	public class ModManifest {
		public string Name;
		public string Description;
		public string Author;
		public string ModVersion;
		public string GameVersion;

		public override string ToString() {
			return $"{nameof(Name)}: {Name}, {nameof(Description)}: {Description}, {nameof(Author)}: {Author}, {nameof(ModVersion)}: {ModVersion}, {nameof(GameVersion)}: {GameVersion}";
		}
	}

	[Serializable]
	public class ModDependency {
		public string Name;
		public string Version;

		public override string ToString() {
			return $"{nameof(Name)}: {Name}, {nameof(Version)}: {Version}";
		}
	}

}
