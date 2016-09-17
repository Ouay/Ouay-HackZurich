﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace Ouay_HackZurich.Verification
{
	class VerificationProfileViewModel : ViewModelBase
	{
		AudioRecorder audioRecorder;
		public VerificationProfileViewModel(OxfordSpeakerIdRestClient oxfordRestClient)
		{
			this.oxfordRestClient = oxfordRestClient;

			this.enrolCommand = new SimpleCommand(this.OnEnrolCommand, false);

			this.verifyCommand = new SimpleCommand(this.OnVerifyCommand, false);
		}

		public VerificationProfile Profile
		{
			get { return (this.profile); }
			set { base.SetProperty(ref this.profile, value); this.EnableEnrolCommand(); this.EnableVerifyCommand(); }
		}

		void EnableEnrolCommand()
		{
			this.enrolCommand.Enable(this.profile.EnrollmentsCount < 3);
		}

		void EnableVerifyCommand()
		{
			this.verifyCommand.Enable(this.profile.EnrollmentsCount >= 3);
		}

		async Task EnrolCommandAsync(Func<IInputStream, Task> innerAction)
		{
			var phrase = await VerificationPhraseList.GetVerificationPhraseForProfileAsync(
			  this.profile);

			audioRecorder = new AudioRecorder();

			var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
			  Guid.NewGuid().ToString());

			await audioRecorder.StartRecordToFileAsync(file);

			this.profile.TextDisplay = "Start to say My name is unknown to you";
			DispatcherTimer time = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 5) };
			time.Tick += async (sender, args) =>
			{
				time.Stop();
				await audioRecorder.StopRecordAsync();

				// Ok, we now have a file full of audio to send to the service.
				using (var stream = await file.OpenReadAsync())
				{
					try
					{
						await innerAction(stream);
					}
					catch (Exception ex)
					{
						await this.ShowErrorAsync(ex.Message);
					}
				}
			};
			time.Start();
		}

		async void OnEnrolCommand()
		{
			await this.EnrolCommandAsync(
			  async (stream) =>
			  {
				  try
				  {
					  var result = await this.oxfordRestClient.EnrollAsync(this.profile, stream);

					  this.profile.EnrollmentStatus = result.EnrollmentStatus;
					  this.profile.EnrollmentsCount = result.EnrollmentsCount;
					  this.profile.RemainingEnrollmentsCount = result.RemainingEnrollments;

					  this.EnableEnrolCommand();
					  this.EnableVerifyCommand();

					  this.profile.TextDisplay = "The service heard you say " + result.Phrase;
				  }
				  catch (Exception ex)
				  {
					  await this.ShowErrorAsync(ex.Message);
				  }
			  }
			);
		}

		async void OnVerifyCommand()
		{
			await this.EnrolCommandAsync(
			  async (stream) =>
			  {
				  try
				  {
					  /*Result return "Accept or Reject*/
					  var result = await this.oxfordRestClient.VerifyAsync(this.profile, stream);
					  await this.ShowErrorAsync("The service heard you say [" + result.Phrase + "] with [" + result.Confidence + "] confidence, it says : [" + result.Result + "]");
					  this.profile.TextDisplay = "The service heard you say " + result.Phrase + " with " + result.Confidence + " confidence";
				  }
				  catch (Exception ex)
				  {
					  await this.ShowErrorAsync(ex.Message);
					  this.profile.TextDisplay = "Error while listening";
				  }
			  }
			);
		}

		async Task ShowErrorAsync(string error)
		{
			var dialog = new MessageDialog(error, "Information");
			await dialog.ShowAsync();
		}

		public ICommand EnrolCommand { get { return (this.enrolCommand); } }

		public ICommand VerifyCommand { get { return (this.verifyCommand); } }

		public object Thread { get; private set; }

		OxfordSpeakerIdRestClient oxfordRestClient;
		VerificationProfile profile;
		SimpleCommand enrolCommand;
		SimpleCommand verifyCommand;
	}
}