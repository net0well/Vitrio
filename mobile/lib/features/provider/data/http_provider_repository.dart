import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/json_body.dart';
import '../domain/availability_slot.dart';
import '../domain/mei_check.dart';
import '../domain/photo_upload.dart';
import '../domain/profile_changes.dart';
import '../domain/provider_dashboard.dart';
import '../domain/provider_photo.dart';
import '../domain/provider_profile.dart';
import '../domain/provider_service.dart';
import '../domain/public_provider.dart';
import '../domain/service_area_draft.dart';
import '../domain/service_category.dart';
import 'provider_repository.dart';

final class HttpProviderRepository implements ProviderRepository {
  const HttpProviderRepository(this._api);

  static const _provider = '/api/v1/provider';

  final ApiClient _api;

  @override
  Future<ApiResult<List<ServiceCategory>>> categories() => _api.get(
        '/api/v1/categories',
        decode: (json) => jsonObjects(json).map(ServiceCategory.fromJson).toList(growable: false),
      );

  @override
  Future<ApiResult<ProviderProfile>> create({required String displayName, required String headline}) async {
    final created = await _api.post(_provider, body: {'displayName': displayName, 'headline': headline}, decode: _profile);
    if (created is ApiSuccess<ProviderProfile>) {
      await _api.renewSession();
    }

    return created;
  }

  @override
  Future<ApiResult<ProviderProfile>> profile() => _api.get(_provider, decode: _profile);

  @override
  Future<ApiResult<ProviderProfile>> update(ProfileChanges changes) => _api.patch(_provider, body: changes.toJson(), decode: _profile);

  @override
  Future<ApiResult<void>> setCategories(List<int> categoryIds, int primaryCategoryId) => _api.put(
        '$_provider/categories',
        body: {'categoryIds': categoryIds, 'primaryCategoryId': primaryCategoryId},
        decode: _ignore,
      );

  @override
  Future<ApiResult<void>> setServiceArea(ServiceAreaDraft area) =>
      _api.put('$_provider/service-area', body: area.toJson(), decode: _ignore);

  @override
  Future<ApiResult<void>> setServices(List<ProviderService> services) => _api.put(
        '$_provider/services',
        body: {'services': services.map((service) => service.toJson()).toList()},
        decode: _ignore,
      );

  @override
  Future<ApiResult<void>> setAvailability(List<AvailabilitySlot> slots, {required bool handlesEmergencies}) => _api.put(
        '$_provider/availability',
        body: {'slots': slots.map((slot) => slot.toJson()).toList(), 'handlesEmergencies': handlesEmergencies},
        decode: _ignore,
      );

  @override
  Future<ApiResult<void>> setAvailableToday(bool value) =>
      _api.put('$_provider/available-today', body: {'value': value}, decode: _ignore);

  @override
  Future<ApiResult<void>> publish() => _api.post('$_provider/publish', decode: _ignore);

  @override
  Future<ApiResult<void>> pause() => _api.post('$_provider/pause', decode: _ignore);

  @override
  Future<ApiResult<ProviderDashboard>> dashboard() =>
      _api.get('$_provider/dashboard', decode: (json) => ProviderDashboard.fromJson(jsonObject(json)));

  @override
  Future<ApiResult<MeiCheck>> verifyMei(String cnpj) =>
      _api.post('$_provider/mei-verification', body: {'cnpj': cnpj}, decode: (json) => MeiCheck.fromJson(jsonObject(json)));

  @override
  Future<ApiResult<List<ProviderPhoto>>> photos() => _api.get(
        '$_provider/photos',
        decode: (json) => jsonObjects(json).map(ProviderPhoto.fromJson).toList(growable: false),
      );

  @override
  Future<ApiResult<PhotoUpload>> requestUpload({required bool avatar, required String contentType, required int contentLength}) =>
      _api.post(
        '$_provider/photos/upload-url',
        body: {'purpose': avatar ? 'avatar' : 'portfolio', 'contentType': contentType, 'contentLength': contentLength},
        decode: (json) => PhotoUpload.fromJson(jsonObject(json)),
      );

  @override
  Future<ApiResult<void>> confirmPhoto(String objectKey) =>
      _api.post('$_provider/photos', body: {'objectKey': objectKey}, decode: _ignore);

  @override
  Future<ApiResult<void>> setAvatar(String objectKey) =>
      _api.put('$_provider/avatar', body: {'objectKey': objectKey}, decode: _ignore);

  @override
  Future<ApiResult<void>> deletePhoto(String photoId) => _api.delete('$_provider/photos/$photoId', decode: _ignore);

  @override
  Future<ApiResult<PublicProvider>> publicProfile(String providerId, {double? latitude, double? longitude}) => _api.get(
        '/api/v1/providers/$providerId',
        query: {
          if (latitude != null && longitude != null) ...{'lat': latitude.toString(), 'lng': longitude.toString()},
        },
        decode: (json) => PublicProvider.fromJson(jsonObject(json)),
      );

  static ProviderProfile _profile(Object? json) => ProviderProfile.fromJson(jsonObject(json));

  static void _ignore(Object? json) {}
}
