import '../../../core/network/api_result.dart';
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

abstract interface class ProviderRepository {
  Future<ApiResult<List<ServiceCategory>>> categories();

  Future<ApiResult<ProviderProfile>> create({required String displayName, required String headline});

  Future<ApiResult<ProviderProfile>> profile();

  Future<ApiResult<ProviderProfile>> update(ProfileChanges changes);

  Future<ApiResult<void>> setCategories(List<int> categoryIds, int primaryCategoryId);

  Future<ApiResult<void>> setServiceArea(ServiceAreaDraft area);

  Future<ApiResult<void>> setServices(List<ProviderService> services);

  Future<ApiResult<void>> setAvailability(List<AvailabilitySlot> slots, {required bool handlesEmergencies});

  Future<ApiResult<void>> setAvailableToday(bool value);

  Future<ApiResult<void>> publish();

  Future<ApiResult<void>> pause();

  Future<ApiResult<ProviderDashboard>> dashboard();

  Future<ApiResult<MeiCheck>> verifyMei(String cnpj);

  Future<ApiResult<List<ProviderPhoto>>> photos();

  Future<ApiResult<PhotoUpload>> requestUpload({required bool avatar, required String contentType, required int contentLength});

  Future<ApiResult<void>> confirmPhoto(String objectKey);

  Future<ApiResult<void>> setAvatar(String objectKey);

  Future<ApiResult<void>> deletePhoto(String photoId);

  Future<ApiResult<PublicProvider>> publicProfile(String providerId, {double? latitude, double? longitude});
}
