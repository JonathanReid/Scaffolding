using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.Audio;
using UnityEngineInternal;

namespace Scaffolding.Audio.Editor
{
	
	public class SoundManagerEditor : EditorWindow {

		private static SoundManagerEditor _window;
		private AudioConfig _config;
		private List<ReorderableList> _sfxLists;
		private Vector2 _scrollArea;
		private bool _createTrigger;
		private bool _createChannel;

		[MenuItem("Tools/Scaffolding/Audio/Open Sound Manager")]
		static void OpenWindow()
		{
			_window = (SoundManagerEditor)EditorWindow.GetWindow(typeof(SoundManagerEditor));
			_window.title = "Audio Library";
		}

		void OnEnable()
		{
			_config = AudioConfig.Get ("SCAudioConfig.asset","ScaffoldingAudio/Resources");
			if(_config.SFXGroups == null)
			{
				_config.SFXGroups = new System.Collections.Generic.List<AudioGroupVO>();
			}

			CreateLists();
		}

		private void AddNewItemToLists()
		{
			AudioGroupVO vo = new AudioGroupVO();
			vo.Clips = new List<AudioVO>();
			_config.SFXGroups.Add(vo);

			CreateLists();
		}

		private void CreateLists()
		{
			_sfxLists = new List<ReorderableList>();
			
			int i = 0, l = _config.SFXGroups.Count;
			foreach(AudioGroupVO group in _config.SFXGroups)
			{
				AudioGroupVO vo = _config.SFXGroups[i];
				ReorderableList Rlist = new ReorderableList(group.Clips,typeof(AudioVO), true, true, true, true);
				Rlist.drawHeaderCallback = (rect) =>
				{

					GUI.Label(rect,"SFX Group");//_config.SFXGroups[i].GroupName);
				};

				Rlist.drawElementCallback =  
				(Rect rect, int index, bool isActive, bool isFocused) => {

					Rlist.list[index] = DrawAudioBar(Rlist,rect,index);
					(Rlist.list[index] as AudioVO).Trigger = vo.Trigger;
				};
				
				_sfxLists.Add(Rlist);
				i++;
			}
		}

		private AudioVO DrawAudioBar(ReorderableList Rlist, Rect rect, int index)
		{
			AudioVO element = (AudioVO)Rlist.list[index];
			rect.y += 2;
			EditorGUI.LabelField(new Rect(rect.x, rect.y, 30, EditorGUIUtility.singleLineHeight),"Clip:");
			rect.x += 30;
			
			float width = rect.width - 270;
			
			AudioClip clip = Resources.Load<AudioClip>(element.Clip);
			
			element.Clip = AssetDatabase.GetAssetPath( (AudioClip)EditorGUI.ObjectField(new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight), clip, typeof(AudioClip)) );
			if(element.Clip.Length > 0)
			{
				element.Clip = element.Clip.Remove(0, element.Clip.IndexOf("Resources") + 10);
				element.Clip = element.Clip.Remove(element.Clip.LastIndexOf("."), element.Clip.Length - element.Clip.LastIndexOf("."));
			}
			rect.x += width;
			EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight),"Volume:");
			rect.x += 50;
			element.ClipVolume = EditorGUI.FloatField(new Rect(rect.x, rect.y, 30, EditorGUIUtility.singleLineHeight), element.ClipVolume);
			rect.x += 40;
			EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight),"Variation:");
			rect.x += 60;
			element.Variation = EditorGUI.FloatField(new Rect(rect.x, rect.y, 30, EditorGUIUtility.singleLineHeight), element.Variation);
			rect.x += 40;
			char playIcon = '\u25B6';
			char stopIcon = '\u25A0';
			if(GUI.Button(new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight),playIcon.ToString()))
			{
				if(clip != null)
				{
					EditorAudio.StopClips();
					EditorAudio.PlayClip(clip);
				}
			}
			
			rect.width -= rect.x + 5;
			rect.x += 25;
			if(GUI.Button(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),stopIcon.ToString()))
			{
				EditorAudio.StopClips();
			}

			return element;
		}

		void OnGUI()
		{
			if(_config.Mixer == null)
			{
				_config.Mixer = Resources.Load<AudioMixer>("MasterAudioMixer");
			}

			DrawToolbar();

			_config.GlobalVolume = EditorGUILayout.Slider("Global Volume:",_config.GlobalVolume,0,1);

			_scrollArea = GUILayout.BeginScrollView(_scrollArea);

			if (!EditorApplication.isCompiling)
			{
				if(_createTrigger)
				{
					DrawTriggerCreation();
				}
				else if(_createChannel)
				{
					DrawChannelCreation();
				}
				else
				{
					if(_sfxLists != null)
					{
						int i = 0, l = _sfxLists.Count;
						for(;i<l;++i)
						{
							DrawAudioGroup(i);
						}
						if(l == 0)
						{
							EditorGUILayout.HelpBox("No groups created yet!",MessageType.Info);
						}
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox("Updating triggers...",MessageType.Info);
			}

			GUILayout.EndScrollView();
			if(GUI.changed)
			{
				EditorUtility.SetDirty(_config);
			}
		}

		private void DrawToolbar()
		{
			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
			if (GUILayout.Button("Create new Audio group", EditorStyles.toolbarButton))
			{
				AddNewItemToLists();
			}
			
			if (GUILayout.Button("Create new Trigger", EditorStyles.toolbarButton))
			{
				_createTrigger = true;
			}

			if (GUILayout.Button("Create new Channel", EditorStyles.toolbarButton))
			{
				_createChannel = true;
			}

			GUILayout.EndHorizontal();
		}

		private string _triggerName;
		private void DrawTriggerCreation()
		{
			GUILayout.BeginVertical(GUI.skin.FindStyle("Box"));
			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
			GUILayout.Label("Create New Trigger");

			if (GUILayout.Button("X", EditorStyles.toolbarButton,GUILayout.MaxWidth(30)))
			{
				_createTrigger = false;
			}
			GUILayout.EndHorizontal();

			char chr = Event.current.character;
			if ( (chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9')) {
				Event.current.character = '\0';
			}

			_triggerName = EditorGUILayout.TextField("Trigger Name:",_triggerName);

			GUILayout.Space(2);

			if(GUILayout.Button("Create Trigger"))
			{

				BuildTriggerClass(CreateAudioTriggerClass(_triggerName), "AudioTriggers");
			}

			GUILayout.Space(2);

			if(GUILayout.Button("Clear Triggers"))
			{
				BuildTriggerClass(CreateBasicTriggerClass(), "AudioTriggers");
			}

			GUILayout.EndVertical();
		}

		private void BuildTriggerClass(string content, string className)
		{
			string path = ScaffoldingUtils.RecursivelyFindAsset("Assets",className);
			
			var writer = new StreamWriter(path);
			writer.Write(content);
			writer.Close();
			writer.Dispose();
			_createTrigger = false;
			_triggerName = "";
			_createChannel = false;
			_channelName = "";
			AssetDatabase.Refresh();
		}

		private string CreateAudioTriggerClass(string newTrigger)
		{
			string[] triggers = Enum.GetNames(typeof(AudioTrigger));

			string c = "public enum AudioTrigger" +
				"\n {";
			for(int i = 0; i < triggers.Length; ++i)
			{
				c += "\n \t" + triggers[i]+",";
			}

			c += "\n \t" + newTrigger;
			c += "\n }";

			return c;
		}

		private string CreateBasicTriggerClass()
		{
			string c = "public enum AudioTrigger" +
				"\n {";
			c += "\n \t BackgroundMusic,";
			c += "\n \t SFX";
			c += "\n }";
			
			return c;
		}

		private string _channelName;
		private void DrawChannelCreation()
		{
			GUILayout.BeginVertical(GUI.skin.FindStyle("Box"));
			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
			GUILayout.Label("Create New Channel");
			
			if (GUILayout.Button("X", EditorStyles.toolbarButton,GUILayout.MaxWidth(30)))
			{
				_createChannel = false;
			}
			GUILayout.EndHorizontal();
			
			char chr = Event.current.character;
			if ( (chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9')) {
				Event.current.character = '\0';
			}
			
			_channelName = EditorGUILayout.TextField("Channel Name:",_channelName);
			
			GUILayout.Space(2);
			
			if(GUILayout.Button("Create Channel"))
			{
				BuildTriggerClass(CreateAudioChannelClass(_channelName),"AudioChannels");
			}
			
			GUILayout.Space(2);
			
			if(GUILayout.Button("Clear Channels"))
			{
				BuildTriggerClass(CreateBasicChannelClass(),"AudioChannels");
			}
			
			GUILayout.EndVertical();
		}

		private string CreateAudioChannelClass(string newChannels)
		{
			string[] channels = Enum.GetNames(typeof(AudioChannel));
			
			string c = "public enum AudioChannel" +
				"\n {";
			for(int i = 0; i < channels.Length; ++i)
			{
				c += "\n \t" + channels[i]+",";
			}
			
			c += "\n \t" + newChannels;
			c += "\n }";
			
			return c;
		}
		
		private string CreateBasicChannelClass()
		{
			string c = "public enum AudioChannel" +
				"\n {";
			c += "\n }";
			
			return c;
		}

		private void DrawAudioGroup(int i)
		{
			AudioGroupVO groupVO = _config.SFXGroups[i];
			GUILayout.BeginVertical(GUI.skin.FindStyle("Box"));
			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
			
			GUILayout.Label("Trigger:",GUILayout.MaxWidth(60));
			
			groupVO.Trigger = (AudioTrigger)EditorGUILayout.EnumPopup(groupVO.Trigger);

			GUILayout.Label("Channel:",GUILayout.MaxWidth(60));
			
			groupVO.Channel = (AudioChannel)EditorGUILayout.EnumPopup(groupVO.Channel);
			
			if (GUILayout.Button(groupVO.Expanded ? "-" : "+", EditorStyles.toolbarButton,GUILayout.MaxWidth(30)))
			{
				groupVO.Expanded = !groupVO.Expanded;
			}
			
			if (GUILayout.Button("X", EditorStyles.toolbarButton,GUILayout.MaxWidth(30)))
			{
				if(EditorUtility.DisplayDialog("Delete audio group","Are you sure you want to delete this audio group?", "OK", "Cancel"))
				{
					_config.SFXGroups.RemoveAt(i);
					_sfxLists.RemoveAt(i);
				}
//				break;
			}
			GUILayout.EndHorizontal();
			
			if(groupVO.Expanded)
			{
				_sfxLists[i].DoLayoutList();
			}
			GUILayout.EndVertical();
			
			_config.SFXGroups[i] = groupVO;
			
			GUILayout.Space(2);
		}
	}
	
}