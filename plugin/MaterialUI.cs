using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.IoC;

namespace MaterialUI {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public class MaterialUI : IDalamudPlugin {
		public string Name => "Material UI";
		private const string command = "/materialui";
		
		public string penumbraIssue {get; private set;} = null;
		
		[PluginService] public IDalamudPluginInterface pluginInterface {get; private set;}
		[PluginService] public ICommandManager commandManager {get; private set;}
		[PluginService] public IPluginLog log { get; set; }
		public Config config {get; private set;}
		[PluginService] public ITextureProvider textureProvider {get; private set;}
		public UI ui {get; private set;}
		public Updater updater {get; private set;}
		
		public MaterialUI() {
			
			config = pluginInterface.GetPluginConfig() as Config ?? new Config();
			updater = new Updater(this);
			ui = new UI(this);
			
			commandManager.AddHandler(command, new CommandInfo(OnCommand) {
				HelpMessage = "Opens the Material UI configuration window"
			});
			
			if(config.openOnStart)
				ui.settingsVisible = true;
			
			Task.Run(async() => {
				for(int i = 0; i < 15; i++) {
					CheckPenumbra();
					if(penumbraIssue == null) {
						updater.Update();
						
						break;
					}
					
					await Task.Delay(1000);
				}
			});
		}
		
		public void Dispose() {
			commandManager.RemoveHandler(command);
			ui.Dispose();
		}
		
		public void CheckPenumbra() {
			try {
				pluginInterface.GetIpcSubscriber<(int, int)>("Penumbra.ApiVersions").InvokeFunc();
			} catch(Exception e) {
				log.Error("Penumbra.ApiVersions failed", e);
				penumbraIssue = "Penumbra not found.";
				
				return;
			}
			
			// string penumbraConfigPath = $"{pluginInterface.ConfigFile.DirectoryName}/Penumbra.json";
			// if (!File.Exists(penumbraConfigPath)) {
			// 	penumbraIssue = "Can't find Penumbra Config.";
			// 	
			// 	return;
			// }
			
			string penumbraPath = pluginInterface.GetIpcSubscriber<string>("Penumbra.GetModDirectory").InvokeFunc();
			if(penumbraPath == "") {
				penumbraIssue = "Penumbra Mod Directory has not been set.";
				
				return;
			}
			
			penumbraIssue = null;
		}
		
		private void OnCommand(string cmd, string args) {
			if(cmd == command)
				ui.settingsVisible = !ui.settingsVisible;
		}
	}
}