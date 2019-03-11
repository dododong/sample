using System.Collections;
using System;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.UI;

public class UIVideoPlayer : UIBase
{
	[SerializeField]
	private VideoClip _movie;

	[SerializeField]
	private RawImage _texture;

	[SerializeField]
	private VideoPlayer _videoPlayer;

	[SerializeField]
	private AudioSource _audioSource;

	private Coroutine _coroutine;

	public void PlayeVideo(string _video, Action callback = null)
	{
		_movie = Resources.Load<VideoClip>(_video);
		if (_movie == null)
		{
			callback?.Invoke();
			return;
		}
		StartCoroutine(PlayVideo(callback));
	}

	private IEnumerator PlayVideo(Action callback = null)
	{
		_videoPlayer.playOnAwake = false;
		_videoPlayer.isLooping = true;
		_audioSource.playOnAwake = false;
		_audioSource.loop = false;
		_audioSource.volume = 0.5f;
		_videoPlayer.source = VideoSource.VideoClip;
		_videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

		_videoPlayer.EnableAudioTrack(0, true);
		_videoPlayer.SetTargetAudioSource(0, _audioSource);

		_videoPlayer.clip = _movie;
		_videoPlayer.Prepare();

		while (!_videoPlayer.isPrepared)
		{
			yield return null;
		}

		_texture.texture = _videoPlayer.texture;

		_videoPlayer.Play();
		_audioSource.Play();
		ScreenEffect.instance.ScreenFadeEffect(ScreenEffect.ScreenEffectType.Screen, Color.black, true);
		while (_videoPlayer.isPlaying)
		{
			yield return null;
		}
		callback?.Invoke();
	}

	public void SpeedManager(float speed)
	{
		if (_coroutine != null)
		{
			StopCoroutine(_coroutine);
			_coroutine = null;

			float revSpeed = 1;
			float time = 0.0f;

			_coroutine = StartCoroutine(CommonMethod.CustomUpdate(0.03f, () =>
		   {
			   _videoPlayer.playbackSpeed = Mathf.Lerp(Mathf.Lerp(revSpeed, speed, time), Mathf.Lerp(speed, revSpeed, time), time);
			   time += Time.deltaTime;
			   if (time > 1.0f)
			   {
				   StopCoroutine(_coroutine);
				   _coroutine = null;
				   _videoPlayer.playbackSpeed = revSpeed;
			   }
		   }));
		}
	}

	private void OnDestroy()
	{
		_videoPlayer.Stop();
		_audioSource.Stop();
		Destroy(_videoPlayer);
		Destroy(_audioSource);
		_videoPlayer = null;
		_audioSource = null;
	}
}