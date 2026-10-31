import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/features/location/domain/city_summary.dart';
import 'package:vitrio/features/provider/data/provider_repository.dart';
import 'package:vitrio/features/provider/domain/availability_slot.dart';
import 'package:vitrio/features/provider/domain/mei_check.dart';
import 'package:vitrio/features/provider/domain/mei_status.dart';
import 'package:vitrio/features/provider/domain/photo_upload.dart';
import 'package:vitrio/features/provider/domain/profile_changes.dart';
import 'package:vitrio/features/provider/domain/provider_category.dart';
import 'package:vitrio/features/provider/domain/provider_dashboard.dart';
import 'package:vitrio/features/provider/domain/provider_photo.dart';
import 'package:vitrio/features/provider/domain/provider_profile.dart';
import 'package:vitrio/features/provider/domain/provider_service.dart';
import 'package:vitrio/features/provider/domain/provider_status.dart';
import 'package:vitrio/features/provider/domain/public_provider.dart';
import 'package:vitrio/features/provider/domain/service_area.dart';
import 'package:vitrio/features/provider/domain/service_area_draft.dart';
import 'package:vitrio/features/provider/domain/service_category.dart';

const jaboticabalCity = CitySummary(ibgeCode: 3524303, name: 'Jaboticabal', state: 'SP');

final class FakeProviderRepository implements ProviderRepository {
  static const _categories = [
    ServiceCategory(id: 1, slug: 'eletrica', name: 'Elétrica', icon: 'bolt'),
    ServiceCategory(id: 2, slug: 'hidraulica', name: 'Hidráulica', icon: 'droplet'),
  ];

  void Function()? onCreated;
  bool exists = false;
  ProviderStatus status = ProviderStatus.draft;
  String displayName = '';
  String headline = '';
  String? about;
  String? contactPhone = '+5516991234567';
  bool whatsAppEnabled = true;
  List<int> categoryIds = [];
  ServiceAreaDraft? area;
  List<ProviderService> services = [];
  bool availableToday = false;
  PublicProvider? published;
  final List<String> calls = [];

  void seedComplete() {
    exists = true;
    displayName = 'Carlos Eletricista';
    headline = 'Eletricista';
    about = 'Eletricista há 12 anos, faço instalações e reparos residenciais com garantia.';
    categoryIds = [1];
    area = const ServiceAreaDraft(cep: '14870000', number: '120', radiusKm: 15, neighborhoods: ['Centro']);
    services = const [ProviderService(title: 'Troca de chuveiro', priceFrom: 80)];
  }

  ProviderProfile get _profile => ProviderProfile(
        id: '0199a0c4-0000-7000-8000-00000000c0de',
        status: status,
        displayName: displayName,
        headline: headline,
        about: about,
        contactPhone: contactPhone,
        whatsAppEnabled: whatsAppEnabled,
        handlesEmergencies: false,
        availableToday: availableToday,
        categories: [
          for (final (index, id) in categoryIds.indexed)
            ProviderCategory(id: id, name: _categories.firstWhere((category) => category.id == id).name, isPrimary: index == 0),
        ],
        serviceArea: area == null
            ? null
            : ServiceArea(
                cep: area!.cep,
                number: area!.number,
                city: jaboticabalCity,
                radiusKm: area!.radiusKm,
                neighborhoods: area!.neighborhoods,
                locationApproximate: false,
              ),
        services: services,
        availability: const [],
        missing: [
          if (about == null) 'about',
          if (categoryIds.isEmpty) 'categories',
          if (area == null) 'serviceArea',
          if (services.isEmpty) 'services',
          if (contactPhone == null) 'contactPhone',
        ],
      );

  ApiResult<T> _ok<T>(String call, T value) {
    calls.add(call);
    return ApiSuccess(value);
  }

  @override
  Future<ApiResult<List<ServiceCategory>>> categories() async => const ApiSuccess(_categories);

  @override
  Future<ApiResult<ProviderProfile>> create({required String displayName, required String headline}) async {
    exists = true;
    this.displayName = displayName;
    this.headline = headline;
    onCreated?.call();
    return _ok('create', _profile);
  }

  @override
  Future<ApiResult<ProviderProfile>> profile() async =>
      exists ? ApiSuccess(_profile) : const ApiFailure(ProblemDetails(status: 404, code: 'provider.not_found'));

  @override
  Future<ApiResult<ProviderProfile>> update(ProfileChanges changes) async {
    displayName = changes.displayName ?? displayName;
    headline = changes.headline ?? headline;
    about = changes.about ?? about;
    contactPhone = changes.contactPhone ?? contactPhone;
    whatsAppEnabled = changes.whatsAppEnabled ?? whatsAppEnabled;
    return _ok('update', _profile);
  }

  @override
  Future<ApiResult<void>> setCategories(List<int> categoryIds, int primaryCategoryId) async {
    this.categoryIds = [...categoryIds];
    return _ok('setCategories', null);
  }

  @override
  Future<ApiResult<void>> setServiceArea(ServiceAreaDraft area) async {
    this.area = area;
    return _ok('setServiceArea', null);
  }

  @override
  Future<ApiResult<void>> setServices(List<ProviderService> services) async {
    this.services = [...services];
    return _ok('setServices', null);
  }

  @override
  Future<ApiResult<void>> setAvailability(List<AvailabilitySlot> slots, {required bool handlesEmergencies}) async =>
      _ok('setAvailability', null);

  @override
  Future<ApiResult<void>> setAvailableToday(bool value) async {
    availableToday = value;
    return _ok('setAvailableToday', null);
  }

  @override
  Future<ApiResult<void>> publish() async {
    status = ProviderStatus.published;
    return _ok('publish', null);
  }

  @override
  Future<ApiResult<void>> pause() async {
    status = ProviderStatus.paused;
    return _ok('pause', null);
  }

  @override
  Future<ApiResult<ProviderDashboard>> dashboard() async => ApiSuccess(ProviderDashboard(
        status: status,
        profileCompleteness: _profile.missing.isEmpty ? 83 : 33,
        nextStep: _profile.missing.isEmpty ? (status == ProviderStatus.published ? 'availability' : 'publish') : _profile.missing.first,
        availableToday: availableToday,
      ));

  @override
  Future<ApiResult<MeiCheck>> verifyMei(String cnpj) async => _ok('verifyMei', const MeiCheck(status: MeiStatus.verified));

  @override
  Future<ApiResult<List<ProviderPhoto>>> photos() async => const ApiSuccess([]);

  @override
  Future<ApiResult<PhotoUpload>> requestUpload({required bool avatar, required String contentType, required int contentLength}) async =>
      _ok('requestUpload', const PhotoUpload(uploadUrl: 'http://storage/upload', objectKey: 'provider/key'));

  @override
  Future<ApiResult<void>> confirmPhoto(String objectKey) async => _ok('confirmPhoto', null);

  @override
  Future<ApiResult<void>> setAvatar(String objectKey) async => _ok('setAvatar', null);

  @override
  Future<ApiResult<void>> deletePhoto(String photoId) async => _ok('deletePhoto', null);

  @override
  Future<ApiResult<PublicProvider>> publicProfile(String providerId, {double? latitude, double? longitude}) async => published == null
      ? const ApiFailure(ProblemDetails(status: 404, code: 'provider.not_found'))
      : ApiSuccess(published!);
}
