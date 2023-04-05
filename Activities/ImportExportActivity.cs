using Android.App;
using Android.Content;
using Android.Net;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Com.Google.Maps.Android.Data.Geojson;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Org.Json;
using SistemaColeta.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using Xamarin.Essentials;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using FluentFTP;

namespace SistemaColeta.Activities
{
	[Activity(Label = "@string/import_export", ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
	public class ImportExportActivity : BaseActivity, View.IOnClickListener
	{
		#region Private fields
		
		#region Protected methods
		protected override void OnCreate(Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_import_export);

			Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);

			_cbUser = FindViewById<AppCompatCheckBox>(Resource.Id.cbUser);
			_cbCollectionData = FindViewById<AppCompatCheckBox>(Resource.Id.cbCollectionData);
			_cbTracking = FindViewById<AppCompatCheckBox>(Resource.Id.cbTracking);
			_cbCollectionPoints = FindViewById<AppCompatCheckBox>(Resource.Id.cbCollectionPoints);

			_cbUserServer = FindViewById<AppCompatCheckBox>(Resource.Id.cbUserServer);
			_cbCollectionDataServer = FindViewById<AppCompatCheckBox>(Resource.Id.cbCollectionDataServer);
			_cbTrackingServer = FindViewById<AppCompatCheckBox>(Resource.Id.cbTrackingServer);
			_cbCollectionPointsServer = FindViewById<AppCompatCheckBox>(Resource.Id.cbCollectionPointsServer);

			_cbUserEmail = FindViewById<AppCompatCheckBox>(Resource.Id.cbUserEmail);
			_cbCollectionDataEmail = FindViewById<AppCompatCheckBox>(Resource.Id.cbCollectionDataEmail);
			_cbTrackingEmail = FindViewById<AppCompatCheckBox>(Resource.Id.cbTrackingEmail);
			_cbCollectionPointsEmail = FindViewById<AppCompatCheckBox>(Resource.Id.cbCollectionPointsEmail);

			FindViewById<AppCompatButton>(Resource.Id.btnClearUser).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnClearAreas).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnClearPoints).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnClearCollectedData).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnClearTracking).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnImportUser).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnImportGeoJson).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnImportImage).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnExport).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnExportServer).SetOnClickListener(this);
			FindViewById<AppCompatButton>(Resource.Id.btnExportEmail).SetOnClickListener(this);

			FindViewById<AppCompatButton>(Resource.Id.btnClearUser).Visibility = DataBase.CurrentUser.Id == Constants.AdminId ? ViewStates.Visible : ViewStates.Gone;
			FindViewById<AppCompatButton>(Resource.Id.btnImportUser).Visibility = DataBase.CurrentUser.Id == Constants.AdminId ? ViewStates.Visible : ViewStates.Gone;
			FindViewById<AppCompatCheckBox>(Resource.Id.cbUser).Visibility = DataBase.CurrentUser.Id == Constants.AdminId ? ViewStates.Visible : ViewStates.Gone;

			if (DataBase.CurrentUser.Id == Constants.AdminId && DataBase.CurrentUser.Name == "Admin")
			{
				FindViewById<AppCompatButton>(Resource.Id.btnImportGeoJson).Enabled = false;
				FindViewById<AppCompatButton>(Resource.Id.btnImportImage).Enabled = false;
				FindViewById<AppCompatButton>(Resource.Id.btnExport).Enabled = false;
				FindViewById<AppCompatButton>(Resource.Id.btnExportServer).Enabled = false;
				FindViewById<AppCompatButton>(Resource.Id.btnExportEmail).Enabled = false;
			}

			FindViewById<AppCompatTextView>(Resource.Id.tvVersion).Text = $"Versão {Xamarin.Essentials.VersionTracking.CurrentVersion}";
		}
		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			try
			{
				switch (requestCode)
				{
					case PICKFILE_USER:
						if (resultCode == Result.Ok)
						{
							List<Uri> uris = new List<Uri>();
							if (data.ClipData != null)
							{
								for (int i = 0; i < data.ClipData.ItemCount; i++)
								{
									uris.Add(data.ClipData.GetItemAt(i).Uri);
								}
							}
							else
							{
								uris.Add(data.Data);
							}

							Task.Run(() => ImportUser(uris));
						}
						break;
					case PICKFILE_GEOJSON:
						if (resultCode == Result.Ok)
						{
							List<Uri> uris = new List<Uri>();
							if (data.ClipData != null)
							{
								for (int i = 0; i < data.ClipData.ItemCount; i++)
								{
									Android.Database.ICursor cursor = ContentResolver.Query(data.ClipData.GetItemAt(i).Uri, null, null, null, null);
									cursor.MoveToFirst();
									if (Path.GetExtension(cursor.GetString(2)).ToLower() != ".geojson".ToLower())
									{
										Task.Run(() => ShowInformationDialog(Resource.String.invalid_format, Resource.String.valid_format));
										return;
									}
									uris.Add(data.ClipData.GetItemAt(i).Uri);
								}
							}
							else
							{
								Android.Database.ICursor cursor = ContentResolver.Query(data.Data, null, null, null, null);
								cursor.MoveToFirst();
								if (Path.GetExtension(cursor.GetString(2)).ToLower() != ".geojson".ToLower())
								{
									Task.Run(() => ShowInformationDialog(Resource.String.invalid_format, Resource.String.valid_format));
									return;
								}
								uris.Add(data.Data);
							}

							Task.Run(() => ImportGeoJson(uris));
						}
						break;
					case PICKFILE_IMAGE:
						if (resultCode == Result.Ok)
						{
							List<Uri> uris = new List<Uri>();
							if (data.ClipData != null)
							{
								for (int i = 0; i < data.ClipData.ItemCount; i++)
								{
									uris.Add(data.ClipData.GetItemAt(i).Uri);
								}
							}
							else
							{
								uris.Add(data.Data);
							}

							Task.Run(() => ImportImage(uris));
						}
						break;
				}
			}
			catch (System.Exception ex)
			{
				Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
			}
			base.OnActivityResult(requestCode, resultCode, data);
		}
		#endregion

		#region public methods
		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					StartActivity(typeof(MapActivity));
					FinishAffinity();
					break;
			}
			return base.OnOptionsItemSelected(item);
		}
		public void OnClick(View v)
		{
			switch (v.Id)
			{
				case Resource.Id.btnClearUser:
					ShowConfirmationDialog(Resource.String.clear_users, Resource.String.you_really_want_to_clean_users, (s, e) => DataBase.ClearUsers());
					break;
				case Resource.Id.btnClearAreas:
					ShowConfirmationDialog(Resource.String.clear_areas, Resource.String.you_really_want_to_clean_areas, (s, e) => DataBase.ClearAreas());
					break;
				case Resource.Id.btnClearPoints:
					ShowConfirmationDialog(Resource.String.clear_points, Resource.String.you_really_want_to_clean_points, (s, e) => DataBase.ClearPoints());
					break;
				case Resource.Id.btnClearCollectedData:
					ShowConfirmationDialog(Resource.String.clear_collected_data, Resource.String.you_really_want_to_clean_points, (s, e) => DataBase.ClearCollectedData());
					break;
				case Resource.Id.btnClearTracking:
					ShowConfirmationDialog(Resource.String.clear_saved_tracks, Resource.String.you_really_want_to_clear_saved_routes, (s, e) => DataBase.ClearTracking());
					break;
				case Resource.Id.btnImportUser:
					Intent intentUser = new Intent();
					intentUser.SetAction(Intent.ActionGetContent);
					intentUser.SetType("application/json");
					intentUser.PutExtra(Intent.ExtraAllowMultiple, true);
					StartActivityForResult(Intent.CreateChooser(intentUser, GetString(Resource.String.select_file)), PICKFILE_USER);
					break;
				case Resource.Id.btnImportGeoJson:
					Intent intentGeoJson = new Intent();
					intentGeoJson.SetAction(Intent.ActionGetContent);
					intentGeoJson.SetType("application/*");
					intentGeoJson.PutExtra(Intent.ExtraAllowMultiple, true);
					StartActivityForResult(Intent.CreateChooser(intentGeoJson, GetString(Resource.String.select_file)), PICKFILE_GEOJSON);
					break;
				case Resource.Id.btnImportImage:
					Intent intentImage = new Intent();
					intentImage.SetAction(Intent.ActionGetContent);
					intentImage.SetType("image/*");
					intentImage.PutExtra(Intent.ExtraAllowMultiple, true);
					StartActivityForResult(Intent.CreateChooser(intentImage, GetString(Resource.String.select_file)), PICKFILE_IMAGE);
					break;
				case Resource.Id.btnExport:
					Export();
					break;
				case Resource.Id.btnExportServer:
					ExportServer();
					break;
				case Resource.Id.btnExportEmail:
					ExportEmail();
					break;
			}
		}
		#endregion

		#region private methods
		private async void ImportUser(List<Uri> uris)
		{
			try
			{
				ShowLoadingDialog(Resource.String.loading);

				string[] jsons = new string[uris.Count];
				for (int i = 0; i < uris.Count; i++)
				{
					Java.IO.InputStream inputStream = ((InputStreamInvoker)ContentResolver.OpenInputStream(uris[i])).BaseInputStream;
					jsons[i] = await Others.Utilities.ReadStreamTextFileAsync(inputStream);
				}

				foreach (string json in jsons)
				{
					List<User> login = DataBase.Login;

					List<User> temp = JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();

					foreach (var item in temp)
					{
						if (login.Find(x => $"{x.Id}" == $"{item.Id}") == null)
						{
							login.Add(item);
						}
					}

					DataBase.Login = login;
				}
			}
			catch (System.Exception ex)
			{
				Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
			}
			finally
			{
				DismissLoadingDialog();
			}
		}
		private async void ImportGeoJson(List<Uri> uris)
		{
			try
			{
				ShowLoadingDialog(Resource.String.loading);

				string[] jsons = new string[uris.Count];
				for (int i = 0; i < uris.Count; i++)
				{
					Java.IO.InputStream inputStream = ((InputStreamInvoker)ContentResolver.OpenInputStream(uris[i])).BaseInputStream;
					jsons[i] = await Others.Utilities.ReadStreamTextFileAsync(inputStream);
				}

				foreach (string json in jsons)
				{
					if (json.Contains("pontos_coleta"))
					{
						#region Verificar campos
						string message = string.Empty;
						GeoJsonLayer layer = new GeoJsonLayer(null, new JSONObject(json));
						foreach (GeoJsonFeature feature in layer.Features.ToEnumerable())
						{
							if (!feature.HasProperty("Id"))
							{
								message += "Campo Id não encontrado.\n";
							}
							if (!feature.HasProperty("Coletado"))
							{
								message += "Campo Coletado não encontrado.\n";
							}
							if (!feature.HasProperty("Talhao"))
							{
								message += "Campo Coletado não encontrado.\n";
							}
							if (!feature.HasProperty("Variedade"))
							{
								message += "Campo Variedade não encontrado.\n";
							}
							if (!feature.HasProperty("Stand"))
							{
								message += "Campo Stand não encontrado.\n";
							}
							if (!feature.HasProperty("Estadio_Fenologico"))
							{
								message += "Campo Estadio_Fenologico não encontrado.\n";
							}
							if (!feature.HasProperty("Observacoes"))
							{
								message += "Campo Observacoes não encontrado.\n";
							}

							if (!string.IsNullOrEmpty(message))
							{
								ShowInformationDialog(GetString(Resource.String.mandatory_properties), message);

								return;
							}
						}
						#endregion

						int countImports = 0;
						int countRepeateds = 0;

						FeatureCollection featureCollectionPoint = DataBase.GeoJsonPoint;
						FeatureCollection temp = JsonConvert.DeserializeObject<FeatureCollection>(json) ?? new FeatureCollection();

						foreach (var item in temp.Features)
						{
							if (featureCollectionPoint.Features.Find(x => $"{x.Properties["Id"]}" == $"{item.Properties["Id"]}") == null)
							{
								featureCollectionPoint.Features.Add(item);
								countImports++;
							}
							else
							{
								countRepeateds++;
							}
						}

						DataBase.GeoJsonPoint = featureCollectionPoint;

						ShowInformationDialog(GetString(Resource.String.success),
							$"{countImports} {GetString(Resource.String.imported_points)}\n" +
							$"{countRepeateds} {GetString(Resource.String.repeated_points)}");
					}
					else
					{
						FeatureCollection featureCollectionArea = DataBase.GeoJsonArea;

						FeatureCollection temp = JsonConvert.DeserializeObject<FeatureCollection>(json) ?? new FeatureCollection();

						foreach (var item in temp.Features)
						{
							featureCollectionArea.Features.Add(item);
						}

						DataBase.GeoJsonArea = featureCollectionArea;
					}
				}
			}
			catch (System.Exception ex)
			{
				Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
			}
			finally
			{
				DismissLoadingDialog();
			}
		}
		private void ImportImage(List<Uri> uris)
		{
			try
			{
				ShowLoadingDialog(Resource.String.loading);

				List<ImageOverlay> images = DataBase.ImageOverlay;

				for (int i = 0; i < uris.Count; i++)
				{
					Android.Database.ICursor cursor = ContentResolver.Query(uris[i], null, null, null, null);
					cursor.MoveToFirst();

					string filename = Path.GetFileNameWithoutExtension(cursor.GetString(2));

					if (double.TryParse(filename.Split(';')[0].Replace('.', ','), out double latitudeSul) &&
					double.TryParse(filename.Split(';')[1].Replace('.', ','), out double longitudeOeste) &&
					double.TryParse(filename.Split(';')[2].Replace('.', ','), out double latitudeNorte) &&
					double.TryParse(filename.Split(';')[3].Replace('.', ','), out double longitudeLeste))
					{
						Stream stream = ContentResolver.OpenInputStream(uris[i]);
						if (stream != null)
						{
							using (MemoryStream memoryStream = new MemoryStream())
							{
								stream.CopyTo(memoryStream);

								images.Add(new ImageOverlay
								{
									Image = memoryStream.ToArray(),
									LatitudeSul = latitudeSul,
									LongitudeOeste = longitudeOeste,
									LatitudeNorte = latitudeNorte,
									LongitudeLeste = longitudeLeste 
								});
							}
						}
					}

				}

				DataBase.ImageOverlay = images;

				ShowInformationDialog("Importação de Imagens", "Imagens foram importadas com sucesso");
			}
			catch (System.Exception ex)
			{
				Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
			}
			finally
			{
				DismissLoadingDialog();
			}
		}
		private async void Export()
		{
			try
			{
				ShowLoadingDialog(Resource.String.loading);

				System.Console.WriteLine();

				string path = await Task.Run(() => $"{GetExternalFilesDir("").AbsolutePath}/{System.DateTime.Now:yyyy-MM-dd_HH:mm}");
				if (!File.Exists(path)) Directory.CreateDirectory(path);

				string dialogMessage = string.Empty;
				ExportUsers(path, ref dialogMessage);
				ExportData(path, ref dialogMessage);
				ExportTracking(path, ref dialogMessage);
				ExportTracking2(path);
				ExportPoints(path, ref dialogMessage);

				ShowInformationDialog(GetString(Resource.String.exported_data), dialogMessage);
			}
			catch (System.Exception ex)
			{
				Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
			}
			finally
			{
				DismissLoadingDialog();
			}
		}

		private void ExportUsers(string path, ref string dialogMessage)
		{
			if (_cbUser.Checked)
			{
				using (StreamWriter writer = new StreamWriter($"{path}/usuários.json"))
				{
					writer.Write(JsonConvert.SerializeObject(DataBase.Login, Formatting.Indented));
				}

				dialogMessage += $"{GetString(Resource.String.users)}: {DataBase.Login.Count}";
			}
		}

		private void ExportPoints(string path, ref string dialogMessage)
		{
			if (_cbCollectionPoints.Checked)
			{
				using (StreamWriter writer = new StreamWriter($"{path}/pontos_coleta.geojson"))
				{
					writer.Write(JsonConvert.SerializeObject(DataBase.GeoJsonPoint, Formatting.Indented));
				}

				dialogMessage += $"\n{GetString(Resource.String.collection_points)}: {DataBase.GeoJsonPoint.Features.Count}";
			}
		}

		private void ExportTracking(string path, ref string dialogMessage)
		{
			System.Console.WriteLine(path);
			if (_cbTracking.Checked)
			{
				FeatureCollection TrackingGeoJson = new FeatureCollection();

				var export = DataBase.SavedTracking;
				foreach (var item in export)
				{
					TrackingGeoJson.Features.Add(item);
				}

				using (StreamWriter writer = new StreamWriter(path: $"{path}/percurso.geojson", append: false))
				{
					writer.Write(JsonConvert.SerializeObject(TrackingGeoJson, Formatting.Indented));
				}

				dialogMessage += $"\n{GetString(Resource.String.routes)}: {export.Count}";
			}
		}
		private void ExportTracking2(string path)
		{
			if (_cbTracking.Checked)
			{
				List<FeatureCollection> featureCollections = new List<FeatureCollection>();

				var export = DataBase.SavedTracking;
				foreach (var item in export)
				{
					var temp = new FeatureCollection();
					temp.Features.Add(item);

					featureCollections.Add(temp);
				}

				using (StreamWriter writer = new StreamWriter(path: $"{path}/percurso.json", append: false))
				{
					writer.Write(JsonConvert.SerializeObject(featureCollections, Formatting.Indented));
				}
			}
		}

		private void ExportData(string path, ref string dialogMessage)
		{
			if (_cbCollectionData.Checked)
			{
				var export = DataBase.SavedPoints;
				if (export.Count > 0)
				{
					FeatureCollection NewGeoJsonPoint = new FeatureCollection();
					FeatureCollection CurrentGeoJsonPoint = DataBase.GeoJsonPoint;

					foreach (var item in export)
					{
						string pathPhotos = $"{path}/imagens";
						if (!File.Exists(pathPhotos)) Directory.CreateDirectory(pathPhotos);

						Feature feature = CurrentGeoJsonPoint.Features.FirstOrDefault(x => $"{x.Properties["Id"]}" == $"{item.IdPoint}");
						var obj = new
						{
							Id = item.IdPoint,
							Usuario = $"{DataBase.CurrentUser.Id} - {DataBase.CurrentUser.Name}",
							Coletado = feature.Properties["Coletado"],
							Localizacao = item.Location,
							Talhao = item.Name,
							Data = item.Date,
							Variedade = item.Variety,
							Stand = item.Stand,
							Estadio_Fenologico = item.PhenologicalStadium,
							Observacoes = item.Observation,
							Sistema_Radicular_Nota = item.RootSystemRating,
							Sistema_Radicular_Observacoes = item.RootSystemObservation,
							Sistema_Radicular_Fotos = new List<string>(),
							Parte_Aerea_Nota = item.AerialPartRating,
							Parte_Aerea_Observacoes = item.AerialPartObservation,
							Parte_Aerea_Fotos = new List<string>(),
							Plantabilidade_Nota = item.PlantabilityRating,
							Plantabilidade_Observacoes = item.PlantabilityObservation,
							Plantabilidade_Fotos = new List<string>(),
							Compactacao_Nota = item.CompactionRating,
							Compactacao_Observacoes = item.CompactionObservation,
							Compactacao_Fotos = new List<string>(),
							Nematoides_Nota = item.NematodesRating,
							Nematoides_Observacoes = item.NematodesObservation,
							Nematoides_Fotos = new List<string>(),
							Outros_Nota = item.OthersRating,
							Outros_Observacoes = item.OthersObservation,
							Outros_Fotos = new List<string>()
						};

						if (File.Exists(item.RootSystemPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto01{Path.GetExtension(item.RootSystemPhoto01)}";
							File.Copy(item.RootSystemPhoto01, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}
						if (File.Exists(item.RootSystemPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto02{Path.GetExtension(item.RootSystemPhoto02)}";
							File.Copy(item.RootSystemPhoto02, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}
						if (File.Exists(item.RootSystemPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto03{Path.GetExtension(item.RootSystemPhoto03)}";
							File.Copy(item.RootSystemPhoto03, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}
						if (File.Exists(item.RootSystemPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto04{Path.GetExtension(item.RootSystemPhoto04)}";
							File.Copy(item.RootSystemPhoto04, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}
						if (File.Exists(item.RootSystemPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto05{Path.GetExtension(item.RootSystemPhoto05)}";
							File.Copy(item.RootSystemPhoto05, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}

						if (File.Exists(item.AerialPartPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto01{Path.GetExtension(item.AerialPartPhoto01)}";
							File.Copy(item.AerialPartPhoto01, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}
						if (File.Exists(item.AerialPartPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto02{Path.GetExtension(item.AerialPartPhoto02)}";
							File.Copy(item.AerialPartPhoto02, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}
						if (File.Exists(item.AerialPartPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto03{Path.GetExtension(item.AerialPartPhoto03)}";
							File.Copy(item.AerialPartPhoto03, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}
						if (File.Exists(item.AerialPartPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto04{Path.GetExtension(item.AerialPartPhoto04)}";
							File.Copy(item.AerialPartPhoto04, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}
						if (File.Exists(item.AerialPartPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto05{Path.GetExtension(item.AerialPartPhoto05)}";
							File.Copy(item.AerialPartPhoto05, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}

						if (File.Exists(item.PlantabilityPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto01{Path.GetExtension(item.PlantabilityPhoto01)}";
							File.Copy(item.PlantabilityPhoto01, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}
						if (File.Exists(item.PlantabilityPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto02{Path.GetExtension(item.PlantabilityPhoto02)}";
							File.Copy(item.PlantabilityPhoto02, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}
						if (File.Exists(item.PlantabilityPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto03{Path.GetExtension(item.PlantabilityPhoto03)}";
							File.Copy(item.PlantabilityPhoto03, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}
						if (File.Exists(item.PlantabilityPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto04{Path.GetExtension(item.PlantabilityPhoto04)}";
							File.Copy(item.PlantabilityPhoto04, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}
						if (File.Exists(item.PlantabilityPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto05{Path.GetExtension(item.PlantabilityPhoto05)}";
							File.Copy(item.PlantabilityPhoto05, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}

						if (File.Exists(item.CompactionPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto01{Path.GetExtension(item.CompactionPhoto01)}";
							File.Copy(item.CompactionPhoto01, name, true);
							obj.Compactacao_Fotos.Add(name);
						}
						if (File.Exists(item.CompactionPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto02{Path.GetExtension(item.CompactionPhoto02)}";
							File.Copy(item.CompactionPhoto02, name, true);
							obj.Compactacao_Fotos.Add(name);
						}
						if (File.Exists(item.CompactionPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto03{Path.GetExtension(item.CompactionPhoto03)}";
							File.Copy(item.CompactionPhoto03, name, true);
							obj.Compactacao_Fotos.Add(name);
						}
						if (File.Exists(item.CompactionPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto04{Path.GetExtension(item.CompactionPhoto04)}";
							File.Copy(item.CompactionPhoto04, name, true);
							obj.Compactacao_Fotos.Add(name);
						}
						if (File.Exists(item.CompactionPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto05{Path.GetExtension(item.CompactionPhoto05)}";
							File.Copy(item.CompactionPhoto05, name, true);
							obj.Compactacao_Fotos.Add(name);
						}

						if (File.Exists(item.NematodesPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto01{Path.GetExtension(item.NematodesPhoto01)}";
							File.Copy(item.NematodesPhoto01, name, true);
							obj.Nematoides_Fotos.Add(name);
						}
						if (File.Exists(item.NematodesPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto02{Path.GetExtension(item.NematodesPhoto02)}";
							File.Copy(item.NematodesPhoto02, name, true);
							obj.Nematoides_Fotos.Add(name);
						}
						if (File.Exists(item.NematodesPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto03{Path.GetExtension(item.NematodesPhoto03)}";
							File.Copy(item.NematodesPhoto03, name, true);
							obj.Nematoides_Fotos.Add(name);
						}
						if (File.Exists(item.NematodesPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto04{Path.GetExtension(item.NematodesPhoto04)}";
							File.Copy(item.NematodesPhoto04, name, true);
							obj.Nematoides_Fotos.Add(name);
						}
						if (File.Exists(item.NematodesPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto05{Path.GetExtension(item.NematodesPhoto05)}";
							File.Copy(item.NematodesPhoto05, name, true);
							obj.Nematoides_Fotos.Add(name);
						}

						if (File.Exists(item.OthersPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto01{Path.GetExtension(item.OthersPhoto01)}";
							File.Copy(item.OthersPhoto01, name, true);
							obj.Outros_Fotos.Add(name);
						}
						if (File.Exists(item.OthersPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto02{Path.GetExtension(item.OthersPhoto02)}";
							File.Copy(item.OthersPhoto02, name, true);
							obj.Outros_Fotos.Add(name);
						}
						if (File.Exists(item.OthersPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto03{Path.GetExtension(item.OthersPhoto03)}";
							File.Copy(item.OthersPhoto03, name, true);
							obj.Outros_Fotos.Add(name);
						}
						if (File.Exists(item.OthersPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto04{Path.GetExtension(item.OthersPhoto04)}";
							File.Copy(item.OthersPhoto04, name, true);
							obj.Outros_Fotos.Add(name);
						}
						if (File.Exists(item.OthersPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto05{Path.GetExtension(item.OthersPhoto05)}";
							File.Copy(item.OthersPhoto05, name, true);
							obj.Outros_Fotos.Add(name);
						}

						NewGeoJsonPoint.Features.Add(new Feature(feature.Geometry, obj));
					}

					using (StreamWriter writer = new StreamWriter(path: $"{path}/dados_coletados.geojson", append: false))
					{
						writer.Write(JsonConvert.SerializeObject(NewGeoJsonPoint, Formatting.Indented));
					}
				};

				dialogMessage += $"\n{GetString(Resource.String.collection_data)}: {export.Count}";
			}
		}

		private async void ExportServer()
		{
			try
			{
				ShowLoadingDialog(Resource.String.loading);

				FindViewById<AppCompatButton>(Resource.Id.btnExportServer).Enabled = false;

				string path = await Task.Run(() => $"{Path.GetTempPath()}/{System.DateTime.Now:yyyy-MM-dd_HH:mm}");
				if (!File.Exists(path)) Directory.CreateDirectory(path);
				var internet = Connectivity.NetworkAccess;

				string pathFtp = $"/{DataBase.CurrentUser.Name}/{System.DateTime.Now:yyyy-MM-dd_HH:mm}";

				if (internet == NetworkAccess.Internet)
				{
					string dialogMessage = string.Empty;

					ExportUsersServer(path, ref dialogMessage, pathFtp);
					ExportDataServer(path, ref dialogMessage, pathFtp);
					ExportTrackingServer(path, ref dialogMessage, pathFtp);
					ExportTracking2Server(path, pathFtp);
					ExportPointsServer(path, ref dialogMessage, pathFtp);

					ShowInformationDialog(GetString(Resource.String.exported_data), dialogMessage);
				}
				else
				{
					string dialogMessage = "Sem acesso a internet, verifique sua conexão!";
					ShowInformationDialog("", dialogMessage);
				}



			}
			catch (System.Exception ex)
			{
				Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
			}
			finally
			{
				FindViewById<AppCompatButton>(Resource.Id.btnExportServer).Enabled = true;
				DismissLoadingDialog();
			}
		}

		private void ExportUsersServer(string path, ref string dialogMessage, string pathFtp)
		{
			if (_cbUserServer.Checked)
			{
				FtpClient client = new FtpClient("ftp.prosap.agr.br", "coleta_informacao@prosap.agr.br", "Pros@p101");
				client.EncryptionMode = FtpEncryptionMode.Auto;
				client.ValidateAnyCertificate = true;
				client.Connect();

				using (StreamWriter writer = new StreamWriter($"{path}/usuarios.json"))
				{
					writer.Write(JsonConvert.SerializeObject(DataBase.Login, Formatting.Indented));
				}

				client.UploadFile($"{path}/usuarios.json", $"{pathFtp}/usuarios.json", FtpRemoteExists.Overwrite, true);

				client.Disconnect();

				dialogMessage += $"{GetString(Resource.String.users)}: {DataBase.Login.Count}";
			}
		}

		private void ExportDataServer(string path, ref string dialogMessage, string pathFtp)
		{
			if (_cbCollectionDataServer.Checked)
			{

				FtpClient client = new FtpClient("ftp.prosap.agr.br", "coleta_informacao@prosap.agr.br", "Pros@p101");
				client.EncryptionMode = FtpEncryptionMode.Auto;
				client.ValidateAnyCertificate = true;
				client.Connect();

				var export = DataBase.SavedPoints;
				if (export.Count > 0)
				{
					FeatureCollection NewGeoJsonPoint = new FeatureCollection();
					FeatureCollection CurrentGeoJsonPoint = DataBase.GeoJsonPoint;

					foreach (var item in export)
					{
						string pathPhotos = $"{path}/imagens";
						if (!File.Exists(pathPhotos)) Directory.CreateDirectory(pathPhotos);

						Feature feature = CurrentGeoJsonPoint.Features.FirstOrDefault(x => $"{x.Properties["Id"]}" == $"{item.IdPoint}");
						var obj = new
						{
							Id = item.IdPoint,
							Usuario = $"{DataBase.CurrentUser.Id} - {DataBase.CurrentUser.Name}",
							Coletado = feature.Properties["Coletado"],
							Localizacao = item.Location,
							Talhao = item.Name,
							Data = item.Date,
							Variedade = item.Variety,
							Stand = item.Stand,
							Estadio_Fenologico = item.PhenologicalStadium,
							Observacoes = item.Observation,
							Sistema_Radicular_Nota = item.RootSystemRating,
							Sistema_Radicular_Observacoes = item.RootSystemObservation,
							Sistema_Radicular_Fotos = new List<string>(),
							Parte_Aerea_Nota = item.AerialPartRating,
							Parte_Aerea_Observacoes = item.AerialPartObservation,
							Parte_Aerea_Fotos = new List<string>(),
							Plantabilidade_Nota = item.PlantabilityRating,
							Plantabilidade_Observacoes = item.PlantabilityObservation,
							Plantabilidade_Fotos = new List<string>(),
							Compactacao_Nota = item.CompactionRating,
							Compactacao_Observacoes = item.CompactionObservation,
							Compactacao_Fotos = new List<string>(),
							Nematoides_Nota = item.NematodesRating,
							Nematoides_Observacoes = item.NematodesObservation,
							Nematoides_Fotos = new List<string>(),
							Outros_Nota = item.OthersRating,
							Outros_Observacoes = item.OthersObservation,
							Outros_Fotos = new List<string>()
						};

						if (File.Exists(item.RootSystemPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto01{Path.GetExtension(item.RootSystemPhoto01)}";
							File.Copy(item.RootSystemPhoto01, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto01{Path.GetExtension(item.RootSystemPhoto01)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.RootSystemPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto02{Path.GetExtension(item.RootSystemPhoto02)}";
							File.Copy(item.RootSystemPhoto02, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto02{Path.GetExtension(item.RootSystemPhoto02)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.RootSystemPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto03{Path.GetExtension(item.RootSystemPhoto03)}";
							File.Copy(item.RootSystemPhoto03, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto03{Path.GetExtension(item.RootSystemPhoto03)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.RootSystemPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto04{Path.GetExtension(item.RootSystemPhoto04)}";
							File.Copy(item.RootSystemPhoto04, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto04{Path.GetExtension(item.RootSystemPhoto04)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.RootSystemPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto05{Path.GetExtension(item.RootSystemPhoto05)}";
							File.Copy(item.RootSystemPhoto05, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto05{Path.GetExtension(item.RootSystemPhoto05)}", FtpRemoteExists.Overwrite, true);
						}

						if (File.Exists(item.AerialPartPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto01{Path.GetExtension(item.AerialPartPhoto01)}";
							File.Copy(item.AerialPartPhoto01, name, true);
							obj.Parte_Aerea_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_aérea_foto01{Path.GetExtension(item.AerialPartPhoto01)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.AerialPartPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto02{Path.GetExtension(item.AerialPartPhoto02)}";
							File.Copy(item.AerialPartPhoto02, name, true);
							obj.Parte_Aerea_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_aérea_foto02{Path.GetExtension(item.AerialPartPhoto02)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.AerialPartPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto03{Path.GetExtension(item.AerialPartPhoto03)}";
							File.Copy(item.AerialPartPhoto03, name, true);
							obj.Parte_Aerea_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_aérea_foto03{Path.GetExtension(item.AerialPartPhoto03)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.AerialPartPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto04{Path.GetExtension(item.AerialPartPhoto04)}";
							File.Copy(item.AerialPartPhoto04, name, true);
							obj.Parte_Aerea_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_aérea_foto04{Path.GetExtension(item.AerialPartPhoto04)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.AerialPartPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto05{Path.GetExtension(item.AerialPartPhoto05)}";
							File.Copy(item.AerialPartPhoto05, name, true);
							obj.Parte_Aerea_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_aérea_foto05{Path.GetExtension(item.AerialPartPhoto05)}", FtpRemoteExists.Overwrite, true);
						}

						if (File.Exists(item.PlantabilityPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto01{Path.GetExtension(item.PlantabilityPhoto01)}";
							File.Copy(item.PlantabilityPhoto01, name, true);
							obj.Plantabilidade_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_plantabilidade_foto01{Path.GetExtension(item.PlantabilityPhoto01)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.PlantabilityPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto02{Path.GetExtension(item.PlantabilityPhoto02)}";
							File.Copy(item.PlantabilityPhoto02, name, true);
							obj.Plantabilidade_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_plantabilidade_foto02{Path.GetExtension(item.PlantabilityPhoto02)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.PlantabilityPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto03{Path.GetExtension(item.PlantabilityPhoto03)}";
							File.Copy(item.PlantabilityPhoto03, name, true);
							obj.Plantabilidade_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_plantabilidade_foto03{Path.GetExtension(item.PlantabilityPhoto03)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.PlantabilityPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto04{Path.GetExtension(item.PlantabilityPhoto04)}";
							File.Copy(item.PlantabilityPhoto04, name, true);
							obj.Plantabilidade_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_plantabilidade_foto04{Path.GetExtension(item.PlantabilityPhoto04)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.PlantabilityPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto05{Path.GetExtension(item.PlantabilityPhoto05)}";
							File.Copy(item.PlantabilityPhoto05, name, true);
							obj.Plantabilidade_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_plantabilidade_foto05{Path.GetExtension(item.PlantabilityPhoto05)}", FtpRemoteExists.Overwrite, true);
						}

						if (File.Exists(item.CompactionPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto01{Path.GetExtension(item.CompactionPhoto01)}";
							File.Copy(item.CompactionPhoto01, name, true);
							obj.Compactacao_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_compactação_foto01{Path.GetExtension(item.CompactionPhoto01)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.CompactionPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto02{Path.GetExtension(item.CompactionPhoto02)}";
							File.Copy(item.CompactionPhoto02, name, true);
							obj.Compactacao_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_compactação_foto02{Path.GetExtension(item.CompactionPhoto02)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.CompactionPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto03{Path.GetExtension(item.CompactionPhoto03)}";
							File.Copy(item.CompactionPhoto03, name, true);
							obj.Compactacao_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_compactação_foto03{Path.GetExtension(item.CompactionPhoto03)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.CompactionPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto04{Path.GetExtension(item.CompactionPhoto04)}";
							File.Copy(item.CompactionPhoto04, name, true);
							obj.Compactacao_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_compactação_foto04{Path.GetExtension(item.CompactionPhoto04)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.CompactionPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto05{Path.GetExtension(item.CompactionPhoto05)}";
							File.Copy(item.CompactionPhoto05, name, true);
							obj.Compactacao_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_compactação_foto05{Path.GetExtension(item.CompactionPhoto05)}", FtpRemoteExists.Overwrite, true);
						}

						if (File.Exists(item.NematodesPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto01{Path.GetExtension(item.NematodesPhoto01)}";
							File.Copy(item.NematodesPhoto01, name, true);
							obj.Nematoides_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_nematóides_foto01{Path.GetExtension(item.NematodesPhoto01)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.NematodesPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto02{Path.GetExtension(item.NematodesPhoto02)}";
							File.Copy(item.NematodesPhoto02, name, true);
							obj.Nematoides_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_nematóides_foto02{Path.GetExtension(item.NematodesPhoto02)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.NematodesPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto03{Path.GetExtension(item.NematodesPhoto03)}";
							File.Copy(item.NematodesPhoto03, name, true);
							obj.Nematoides_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_nematóides_foto03{Path.GetExtension(item.NematodesPhoto03)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.NematodesPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto04{Path.GetExtension(item.NematodesPhoto04)}";
							File.Copy(item.NematodesPhoto04, name, true);
							obj.Nematoides_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_nematóides_foto04{Path.GetExtension(item.NematodesPhoto04)}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.NematodesPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto05{Path.GetExtension(item.NematodesPhoto05)}";
							File.Copy(item.NematodesPhoto05, name, true);
							obj.Nematoides_Fotos.Add(name);

							client.UploadFile($"{name}", $"{pathFtp}/images/ponto_de_coleta_{item.IdPoint}_sistema_nematóides_foto05W{Path.GetExtension(item.NematodesPhoto05)}", FtpRemoteExists.Overwrite, true);
						}

						if (File.Exists(item.OthersPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto01{Path.GetExtension(item.OthersPhoto01)}";
							File.Copy(item.OthersPhoto01, name, true);
							obj.Outros_Fotos.Add(name);

							client.UploadFile($"{path}/{name}", $"{pathFtp}/images/{name}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.OthersPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto02{Path.GetExtension(item.OthersPhoto02)}";
							File.Copy(item.OthersPhoto02, name, true);
							obj.Outros_Fotos.Add(name);

							client.UploadFile($"{path}/{name}", $"{pathFtp}/images/{name}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.OthersPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto03{Path.GetExtension(item.OthersPhoto03)}";
							File.Copy(item.OthersPhoto03, name, true);
							obj.Outros_Fotos.Add(name);

							client.UploadFile($"{path}/{name}", $"{pathFtp}/images/{name}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.OthersPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto04{Path.GetExtension(item.OthersPhoto04)}";
							File.Copy(item.OthersPhoto04, name, true);
							obj.Outros_Fotos.Add(name);

							client.UploadFile($"{path}/{name}", $"{pathFtp}/images/{name}", FtpRemoteExists.Overwrite, true);
						}
						if (File.Exists(item.OthersPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto05{Path.GetExtension(item.OthersPhoto05)}";
							File.Copy(item.OthersPhoto05, name, true);
							obj.Outros_Fotos.Add(name);

							client.UploadFile($"{path}/{name}", $"{pathFtp}/images/{name}", FtpRemoteExists.Overwrite, true);
						}

						NewGeoJsonPoint.Features.Add(new Feature(feature.Geometry, obj));
					}

					using (StreamWriter writer = new StreamWriter(path: $"{path}/dados_coletados.geojson", append: false))
					{
						writer.Write(JsonConvert.SerializeObject(NewGeoJsonPoint, Formatting.Indented));
					}

					client.UploadFile($"{path}/dados_coletados.geojson", $"{pathFtp}/dados_coletados.geojson", FtpRemoteExists.Overwrite, true);

					client.Disconnect();
				};

				dialogMessage += $"\n{GetString(Resource.String.collection_data)}: {export.Count}";
			}
		}

		private void ExportTrackingServer(string path, ref string dialogMessage, string pathFtp)
		{
			if (_cbTrackingServer.Checked)
			{
				string pathGeo = "";
				//GeoTiffRasterLayer geoTiffRasterLayer = new GeoTiffRasterLayer(pathGeo);

				FtpClient client = new FtpClient("ftp.prosap.agr.br", "coleta_informacao@prosap.agr.br", "Pros@p101");
				client.EncryptionMode = FtpEncryptionMode.Auto;
				client.ValidateAnyCertificate = true;
				client.Connect();

				FeatureCollection TrackingGeoJson = new FeatureCollection();

				var export = DataBase.SavedTracking;
				foreach (var item in export)
				{
					TrackingGeoJson.Features.Add(item);
				}

				using (StreamWriter writer = new StreamWriter(path: $"{path}/percurso.geojson", append: false))
				{
					writer.Write(JsonConvert.SerializeObject(TrackingGeoJson, Formatting.Indented));
				}

				client.UploadFile($"{path}/percurso.geojson", $"{pathFtp}/percurso.geojson", FtpRemoteExists.Overwrite, true);

				client.Disconnect();

				dialogMessage += $"\n{GetString(Resource.String.routes)}: {export.Count}";
			}
		}
		private void ExportTracking2Server(string path, string pathFtp)
		{
			if (_cbTrackingServer.Checked)
			{
				FtpClient client = new FtpClient("ftp.prosap.agr.br", "coleta_informacao@prosap.agr.br", "Pros@p101");
				client.EncryptionMode = FtpEncryptionMode.Auto;
				client.ValidateAnyCertificate = true;
				client.Connect();

				List<FeatureCollection> featureCollections = new List<FeatureCollection>();

				var export = DataBase.SavedTracking;
				foreach (var item in export)
				{
					var temp = new FeatureCollection();
					temp.Features.Add(item);

					featureCollections.Add(temp);
				}

				using (StreamWriter writer = new StreamWriter(path: $"{path}/percurso.json", append: false))
				{
					writer.Write(JsonConvert.SerializeObject(featureCollections, Formatting.Indented));
				}

				client.UploadFile($"{path}/percurso.json", $"{pathFtp}/percurso.json", FtpRemoteExists.Overwrite, true);

				client.Disconnect();
			}
		}

		private void ExportPointsServer(string path, ref string dialogMessage, string pathFtp)
		{
			if (_cbCollectionPointsServer.Checked)
			{
				FtpClient client = new FtpClient("ftp.prosap.agr.br", "coleta_informacao@prosap.agr.br", "Pros@p101");
				client.EncryptionMode = FtpEncryptionMode.Auto;
				client.ValidateAnyCertificate = true;
				client.Connect();

				using (StreamWriter writer = new StreamWriter($"{path}/pontos_coleta.geojson"))
				{
					writer.Write(JsonConvert.SerializeObject(DataBase.GeoJsonPoint, Formatting.Indented));
				}

				client.UploadFile($"{path}/pontos_coleta.geojson", $"{pathFtp}/pontos_coleta.geojson", FtpRemoteExists.Overwrite, true);

				client.Disconnect();

				dialogMessage += $"\n{GetString(Resource.String.collection_points)}: {DataBase.GeoJsonPoint.Features.Count}";
			}
		}

		private async void ExportEmail()
		{
			try
			{
				ShowLoadingDialog(Resource.String.loading);

				FindViewById<AppCompatButton>(Resource.Id.btnExportEmail).Enabled = false;

				string path = await Task.Run(() => $"{Path.GetTempPath()}/{System.DateTime.Now:yyyy-MM-dd_HH:mm}");
				if (!File.Exists(path)) Directory.CreateDirectory(path);
				var internet = Connectivity.NetworkAccess;

				if (internet == NetworkAccess.Internet)
				{
					string dialogMessage = string.Empty;

					ExportUsersEmail(path, ref dialogMessage);
					ExportDataEmail(path, ref dialogMessage);
					ExportTrackingEmail(path, ref dialogMessage);
					ExportPointsEmail(path, ref dialogMessage);

					ShowInformationDialog(GetString(Resource.String.exported_data), dialogMessage);
				}
				else
				{
					string dialogMessage = "Sem acesso a internet, verifique sua conexão!";
					ShowInformationDialog("", dialogMessage);
				}



			}
			catch (System.Exception ex)
			{
				Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
			}
			finally
			{
				FindViewById<AppCompatButton>(Resource.Id.btnExportEmail).Enabled = true;
				DismissLoadingDialog();
			}
		}

		private void ExportUsersEmail(string path, ref string dialogMessage)
		{
			if (_cbUserEmail.Checked)
			{
				var message = new MimeMessage();
				message.From.Add(new MailboxAddress("Savana", "coleta_informacao@prosap.agr.br"));
				message.To.Add(new MailboxAddress("To", "coleta_informacao@prosap.agr.br"));
				message.Cc.Add(new MailboxAddress("Cc", "jluisduval@gmail.com"));
				message.Subject = "Exportação de usuários";
				var body = new TextPart("plain")
				{
					Text = $"Usuário: {DataBase.CurrentUser.Id} - {DataBase.CurrentUser.Name}\nData de Exportação: {System.DateTime.Now:dd/MM/yyyy HH:mm}\nE-mail com exportação de usuários"
				};

				using (StreamWriter writer = new StreamWriter($"{path}/usuarios.json"))
				{
					writer.Write(JsonConvert.SerializeObject(DataBase.Login, Formatting.Indented));
				}

				var attachment = new MimePart("json", "geojson")
				{
					ContentObject = new ContentObject(File.OpenRead($"{path}/usuarios.json"), ContentEncoding.Default),
					ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
					FileName = Path.GetFileName($"{path}/usuarios.json")
				};

				var multipart = new Multipart("mixed");
				multipart.Add(body);
				multipart.Add(attachment);

				// now set the multipart/mixed as the message body
				message.Body = multipart;

				using (var client = new SmtpClient())
				{
					// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)  
					client.ServerCertificateValidationCallback = (s, c, h, e) => true;
					client.Connect("mail.prosap.agr.br", 465, true);
					// Note: only needed if the SMTP server requires authentication  
					client.Authenticate("coleta_informacao@prosap.agr.br", "Pros@p101");
					client.Send(message);
					client.Disconnect(true);
				}

				dialogMessage += "\nE-mail de exportação de usuários enviado com sucesso!";
			}
		}

		private void ExportDataEmail(string path, ref string dialogMessage)
		{
			if (_cbCollectionDataEmail.Checked)
			{
				var export = DataBase.SavedPoints;
				if (export.Count > 0)
				{
					FeatureCollection NewGeoJsonPoint = new FeatureCollection();
					FeatureCollection CurrentGeoJsonPoint = DataBase.GeoJsonPoint;

					string pathPhotos = $"{path}/imagens";
					if (!File.Exists(pathPhotos)) Directory.CreateDirectory(pathPhotos);

					var message = new MimeMessage();
					message.From.Add(new MailboxAddress("Savana", "coleta_informacao@prosap.agr.br"));
					message.To.Add(new MailboxAddress("To", "coleta_informacao@prosap.agr.br"));
					message.Cc.Add(new MailboxAddress("Cc", "jluisduval@gmail.com"));
					message.Subject = "Informações coleta";
					var body = new TextPart("plain")
					{
						Text = $"Usuário: {DataBase.CurrentUser.Id} - {DataBase.CurrentUser.Name}\nData Exportação: {System.DateTime.Now:dd/MM/yyyy HH:mm}\nE-mail com informações dos dados da coleta"
					};
					var multipart = new Multipart("mixed");

					multipart.Add(body);

					foreach (var item in export)
					{
						Feature feature = CurrentGeoJsonPoint.Features.FirstOrDefault(x => $"{x.Properties["Id"]}" == $"{item.IdPoint}");
						var obj = new
						{
							Id = item.IdPoint,
							Usuario = $"{DataBase.CurrentUser.Id} - {DataBase.CurrentUser.Name}",
							Coletado = feature.Properties["Coletado"],
							Localizacao = item.Location,
							Talhao = item.Name,
							Data = item.Date,
							Variedade = item.Variety,
							Stand = item.Stand,
							Estadio_Fenologico = item.PhenologicalStadium,
							Observacoes = item.Observation,
							Sistema_Radicular_Nota = item.RootSystemRating,
							Sistema_Radicular_Observacoes = item.RootSystemObservation,
							Sistema_Radicular_Fotos = new List<string>(),
							Parte_Aerea_Nota = item.AerialPartRating,
							Parte_Aerea_Observacoes = item.AerialPartObservation,
							Parte_Aerea_Fotos = new List<string>(),
							Plantabilidade_Nota = item.PlantabilityRating,
							Plantabilidade_Observacoes = item.PlantabilityObservation,
							Plantabilidade_Fotos = new List<string>(),
							Compactacao_Nota = item.CompactionRating,
							Compactacao_Observacoes = item.CompactionObservation,
							Compactacao_Fotos = new List<string>(),
							Nematoides_Nota = item.NematodesRating,
							Nematoides_Observacoes = item.NematodesObservation,
							Nematoides_Fotos = new List<string>(),
							Outros_Nota = item.OthersRating,
							Outros_Observacoes = item.OthersObservation,
							Outros_Fotos = new List<string>()
						};

						if (File.Exists(item.RootSystemPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto01{Path.GetExtension(item.RootSystemPhoto01)}";
							File.Copy(item.RootSystemPhoto01, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}
						if (File.Exists(item.RootSystemPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto02{Path.GetExtension(item.RootSystemPhoto02)}";
							File.Copy(item.RootSystemPhoto02, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}
						if (File.Exists(item.RootSystemPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto03{Path.GetExtension(item.RootSystemPhoto03)}";
							File.Copy(item.RootSystemPhoto03, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}
						if (File.Exists(item.RootSystemPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto04{Path.GetExtension(item.RootSystemPhoto04)}";
							File.Copy(item.RootSystemPhoto04, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}
						if (File.Exists(item.RootSystemPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_sistema_radicular_foto05{Path.GetExtension(item.RootSystemPhoto05)}";
							File.Copy(item.RootSystemPhoto05, name, true);
							obj.Sistema_Radicular_Fotos.Add(name);
						}

						if (File.Exists(item.AerialPartPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto01{Path.GetExtension(item.AerialPartPhoto01)}";
							File.Copy(item.AerialPartPhoto01, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}
						if (File.Exists(item.AerialPartPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto02{Path.GetExtension(item.AerialPartPhoto02)}";
							File.Copy(item.AerialPartPhoto02, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}
						if (File.Exists(item.AerialPartPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto03{Path.GetExtension(item.AerialPartPhoto03)}";
							File.Copy(item.AerialPartPhoto03, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}
						if (File.Exists(item.AerialPartPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto04{Path.GetExtension(item.AerialPartPhoto04)}";
							File.Copy(item.AerialPartPhoto04, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}
						if (File.Exists(item.AerialPartPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_parte_aérea_foto05{Path.GetExtension(item.AerialPartPhoto05)}";
							File.Copy(item.AerialPartPhoto05, name, true);
							obj.Parte_Aerea_Fotos.Add(name);
						}

						if (File.Exists(item.PlantabilityPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto01{Path.GetExtension(item.PlantabilityPhoto01)}";
							File.Copy(item.PlantabilityPhoto01, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}
						if (File.Exists(item.PlantabilityPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto02{Path.GetExtension(item.PlantabilityPhoto02)}";
							File.Copy(item.PlantabilityPhoto02, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}
						if (File.Exists(item.PlantabilityPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto03{Path.GetExtension(item.PlantabilityPhoto03)}";
							File.Copy(item.PlantabilityPhoto03, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}
						if (File.Exists(item.PlantabilityPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto04{Path.GetExtension(item.PlantabilityPhoto04)}";
							File.Copy(item.PlantabilityPhoto04, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}
						if (File.Exists(item.PlantabilityPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_plantabilidade_foto05{Path.GetExtension(item.PlantabilityPhoto05)}";
							File.Copy(item.PlantabilityPhoto05, name, true);
							obj.Plantabilidade_Fotos.Add(name);
						}

						if (File.Exists(item.CompactionPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto01{Path.GetExtension(item.CompactionPhoto01)}";
							File.Copy(item.CompactionPhoto01, name, true);
							obj.Compactacao_Fotos.Add(name);
						}
						if (File.Exists(item.CompactionPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto02{Path.GetExtension(item.CompactionPhoto02)}";
							File.Copy(item.CompactionPhoto02, name, true);
							obj.Compactacao_Fotos.Add(name);
						}
						if (File.Exists(item.CompactionPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto03{Path.GetExtension(item.CompactionPhoto03)}";
							File.Copy(item.CompactionPhoto03, name, true);
							obj.Compactacao_Fotos.Add(name);
						}
						if (File.Exists(item.CompactionPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto04{Path.GetExtension(item.CompactionPhoto04)}";
							File.Copy(item.CompactionPhoto04, name, true);
							obj.Compactacao_Fotos.Add(name);
						}
						if (File.Exists(item.CompactionPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_compactação_foto05{Path.GetExtension(item.CompactionPhoto05)}";
							File.Copy(item.CompactionPhoto05, name, true);
							obj.Compactacao_Fotos.Add(name);
						}

						if (File.Exists(item.NematodesPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto01{Path.GetExtension(item.NematodesPhoto01)}";
							File.Copy(item.NematodesPhoto01, name, true);
							obj.Nematoides_Fotos.Add(name);
						}
						if (File.Exists(item.NematodesPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto02{Path.GetExtension(item.NematodesPhoto02)}";
							File.Copy(item.NematodesPhoto02, name, true);
							obj.Nematoides_Fotos.Add(name);
						}
						if (File.Exists(item.NematodesPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto03{Path.GetExtension(item.NematodesPhoto03)}";
							File.Copy(item.NematodesPhoto03, name, true);
							obj.Nematoides_Fotos.Add(name);
						}
						if (File.Exists(item.NematodesPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto04{Path.GetExtension(item.NematodesPhoto04)}";
							File.Copy(item.NematodesPhoto04, name, true);
							obj.Nematoides_Fotos.Add(name);
						}
						if (File.Exists(item.NematodesPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_nematóides_foto05{Path.GetExtension(item.NematodesPhoto05)}";
							File.Copy(item.NematodesPhoto05, name, true);
							obj.Nematoides_Fotos.Add(name);
						}

						if (File.Exists(item.OthersPhoto01))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto01{Path.GetExtension(item.OthersPhoto01)}";
							File.Copy(item.OthersPhoto01, name, true);
							obj.Outros_Fotos.Add(name);
						}
						if (File.Exists(item.OthersPhoto02))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto02{Path.GetExtension(item.OthersPhoto02)}";
							File.Copy(item.OthersPhoto02, name, true);
							obj.Outros_Fotos.Add(name);
						}
						if (File.Exists(item.OthersPhoto03))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto03{Path.GetExtension(item.OthersPhoto03)}";
							File.Copy(item.OthersPhoto03, name, true);
							obj.Outros_Fotos.Add(name);
						}
						if (File.Exists(item.OthersPhoto04))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto04{Path.GetExtension(item.OthersPhoto04)}";
							File.Copy(item.OthersPhoto04, name, true);
							obj.Outros_Fotos.Add(name);
						}
						if (File.Exists(item.OthersPhoto05))
						{
							var name = $"{pathPhotos}/ponto_de_coleta_{item.IdPoint}_outros_foto05{Path.GetExtension(item.OthersPhoto05)}";
							File.Copy(item.OthersPhoto05, name, true);
							obj.Outros_Fotos.Add(name);
						}

						NewGeoJsonPoint.Features.Add(new Feature(feature.Geometry, obj));
					}

					using (StreamWriter writer = new StreamWriter(path: $"{path}/dados_coletados.geojson", append: false))
					{
						writer.Write(JsonConvert.SerializeObject(NewGeoJsonPoint, Formatting.Indented));
					}

					if (File.Exists($"{path}/dados_coletados.geojson"))
					{
						var attachment = new MimePart("json", "geojson")
						{
							ContentObject = new ContentObject(File.OpenRead($"{path}/dados_coletados.geojson"), ContentEncoding.Default),
							ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
							FileName = Path.GetFileName($"{path}/dados_coletados.geojson")
						};
						multipart.Add(attachment);
					}

					var images = Directory.EnumerateFiles(pathPhotos);
					int num = 0;

					foreach (string currentImage in images)
					{
						num++;
						var attachmentImage = new MimePart("image", "gif")
						{
							ContentObject = new ContentObject(File.OpenRead(currentImage), ContentEncoding.Default),
							ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
							ContentTransferEncoding = ContentEncoding.Base64,
							FileName = Path.GetFileName(currentImage)
						};
						multipart.Add(attachmentImage);
					}

					message.Body = multipart;

					using (var client = new SmtpClient())
					{
						// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)  
						client.ServerCertificateValidationCallback = (s, c, h, e) => true;
						client.Connect("mail.prosap.agr.br", 465, true);
						// Note: only needed if the SMTP server requires authentication  
						client.Authenticate("coleta_informacao@prosap.agr.br", "Pros@p101");
						client.Send(message);
						client.Disconnect(true);
					}

					dialogMessage += "\nE-mail de dados da coleta enviado com sucesso!";
				};
			}
		}

		private void ExportTrackingEmail(string path, ref string dialogMessage)
		{
			if (_cbTrackingEmail.Checked)
			{
				var message = new MimeMessage();
				message.From.Add(new MailboxAddress("Savana", "coleta_informacao@prosap.agr.br"));
				message.To.Add(new MailboxAddress("To", "coleta_informacao@prosap.agr.br"));
				message.Cc.Add(new MailboxAddress("Cc", "jluisduval@gmail.com"));
				message.Subject = "Informações coleta";
				var body = new TextPart("plain")
				{
					Text = $"Usuário: {DataBase.CurrentUser.Id} - {DataBase.CurrentUser.Name}\nData Exportação: {System.DateTime.Now:dd/MM/yyyy HH:mm}\nE-mail com informações do percurso"
				};
				var multipart = new Multipart("mixed");

				multipart.Add(body);

				FeatureCollection TrackingGeoJson = new FeatureCollection();

				var export = DataBase.SavedTracking;
				foreach (var item in export)
				{
					TrackingGeoJson.Features.Add(item);
				}

				using (StreamWriter writer = new StreamWriter(path: $"{path}/percurso.geojson", append: false))
				{
					writer.Write(JsonConvert.SerializeObject(TrackingGeoJson, Formatting.Indented));
				}

				if (File.Exists($"{path}/percurso.geojson"))
				{
					var attachment = new MimePart("json", "geojson")
					{
						ContentObject = new ContentObject(File.OpenRead($"{path}/percurso.geojson"), ContentEncoding.Default),
						ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
						FileName = Path.GetFileName($"{path}/percurso.geojson")
					};
					multipart.Add(attachment);
				}

				List<FeatureCollection> featureCollections = new List<FeatureCollection>();
				var export2 = DataBase.SavedTracking;
				foreach (var item in export2)
				{
					var temp = new FeatureCollection();
					temp.Features.Add(item);

					featureCollections.Add(temp);
				}

				using (StreamWriter writer = new StreamWriter(path: $"{path}/percurso.json", append: false))
				{
					writer.Write(JsonConvert.SerializeObject(featureCollections, Formatting.Indented));
				}

				if (File.Exists($"{path}/percurso.json"))
				{
					var attachment2 = new MimePart("json", "geojson")
					{
						ContentObject = new ContentObject(File.OpenRead($"{path}/percurso.json"), ContentEncoding.Default),
						ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
						FileName = Path.GetFileName($"{path}/percurso.json")
					};
					multipart.Add(attachment2);
				}

				message.Body = multipart;

				using (var client = new SmtpClient())
				{
					// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)  
					client.ServerCertificateValidationCallback = (s, c, h, e) => true;
					client.Connect("mail.prosap.agr.br", 465, true);
					// Note: only needed if the SMTP server requires authentication  
					client.Authenticate("coleta_informacao@prosap.agr.br", "Pros@p101");
					client.Send(message);
					client.Disconnect(true);
				}

				dialogMessage += "\nE-mail de Percurso Enviado com sucesso!";
			}
		}

		private void ExportPointsEmail(string path, ref string dialogMessage)
		{
			if (_cbCollectionPointsEmail.Checked)
			{
				var message = new MimeMessage();
				message.From.Add(new MailboxAddress("Savana", "coleta_informacao@prosap.agr.br"));
				message.To.Add(new MailboxAddress("To", "coleta_informacao@prosap.agr.br"));
				message.Cc.Add(new MailboxAddress("Cc", "jluisduval@gmail.com"));
				message.Subject = "Informações coleta";
				var body = new TextPart("plain")
				{
					Text = $"Usuário: {DataBase.CurrentUser.Id} - {DataBase.CurrentUser.Name}\nData de Exportação: {System.DateTime.Now:dd/MM/yyyy HH:mm}\nE-mail com informações dos pontos de coleta"
				};

				using (StreamWriter writer = new StreamWriter($"{path}/pontos_coleta.geojson"))
				{
					writer.Write(JsonConvert.SerializeObject(DataBase.GeoJsonPoint, Formatting.Indented));
				}

				var attachment = new MimePart("json", "geojson")
				{
					ContentObject = new ContentObject(File.OpenRead($"{path}/pontos_coleta.geojson"), ContentEncoding.Default),
					ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
					FileName = Path.GetFileName($"{path}/pontos_coleta.geojson")
				};

				var multipart = new Multipart("mixed");
				multipart.Add(body);
				multipart.Add(attachment);

				// now set the multipart/mixed as the message body
				message.Body = multipart;

				using (var client = new SmtpClient())
				{
					// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)  
					client.ServerCertificateValidationCallback = (s, c, h, e) => true;
					client.Connect("mail.prosap.agr.br", 465, true);
					// Note: only needed if the SMTP server requires authentication  
					client.Authenticate("coleta_informacao@prosap.agr.br", "Pros@p101");
					client.Send(message);
					client.Disconnect(true);
				}

				dialogMessage += "\nE-mail Pontos de Coleta Enviado com sucesso!";
			}
		}
		#endregion
	}

}