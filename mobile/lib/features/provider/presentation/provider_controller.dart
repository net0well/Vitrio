import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../auth/presentation/session_controller.dart';
import '../data/provider_providers.dart';
import '../data/provider_repository.dart';
import '../domain/photo_upload.dart';
import '../domain/picked_photo.dart';
import '../domain/provider_profile.dart';
import 'photo_outcome.dart';

part 'provider_controller.g.dart';

@Riverpod(keepAlive: true)
class ProviderController extends _$ProviderController {
  @override
  Future<ProviderProfile?> build() async {
    final me = await ref.watch(sessionControllerProvider.future);
    if (me == null || !me.hasProviderProfile) {
      return null;
    }

    return _fetch();
  }

  ProviderRepository get _repository => ref.read(providerRepositoryProvider);

  Future<ApiResult<void>> create({required String displayName, required String headline}) async {
    final created = await _repository.create(displayName: displayName, headline: headline);
    if (created case ApiFailure(:final problem)) {
      return ApiFailure(problem);
    }

    await ref.read(sessionControllerProvider.notifier).reload();
    return const ApiSuccess(null);
  }

  Future<ApiResult<void>> run(Future<ApiResult<Object?>> Function(ProviderRepository repository) action) async {
    final result = await action(_repository);
    if (result case ApiFailure(:final problem)) {
      return ApiFailure(problem);
    }

    state = AsyncData(await _fetch());
    return const ApiSuccess(null);
  }

  Future<PhotoOutcome> addPhoto({required bool avatar}) async {
    final photo = await ref.read(photoPickerProvider).pick();
    return photo == null ? PhotoOutcome.cancelled : uploadPhoto(photo, avatar: avatar);
  }

  Future<PhotoOutcome> uploadPhoto(PickedPhoto photo, {required bool avatar}) async {
    final upload = await _repository.requestUpload(avatar: avatar, contentType: photo.contentType, contentLength: photo.bytes.length);
    if (upload is! ApiSuccess<PhotoUpload> || !await ref.read(photoStorageProvider).upload(upload.data, photo)) {
      return PhotoOutcome.failed;
    }

    final key = upload.data.objectKey;
    final confirmed = avatar ? await _repository.setAvatar(key) : await _repository.confirmPhoto(key);
    return confirmed is ApiSuccess ? PhotoOutcome.added : PhotoOutcome.failed;
  }

  Future<ProviderProfile?> _fetch() async => switch (await _repository.profile()) {
        ApiSuccess(:final data) => data,
        ApiFailure(:final problem) when problem.status == 403 || problem.status == 404 => null,
        ApiFailure(:final problem) => throw problem,
      };
}
