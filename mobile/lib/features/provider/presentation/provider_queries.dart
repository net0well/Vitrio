import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../search/presentation/search_queries.dart';
import '../data/provider_providers.dart';
import '../domain/provider_dashboard.dart';
import '../domain/provider_photo.dart';
import '../domain/public_provider.dart';
import '../domain/service_category.dart';
import 'provider_controller.dart';

part 'provider_queries.g.dart';

@Riverpod(keepAlive: true)
Future<List<ServiceCategory>> serviceCategories(Ref ref) async => switch (await ref.read(providerRepositoryProvider).categories()) {
      ApiSuccess(:final data) => data,
      ApiFailure(:final problem) => throw problem,
    };

@riverpod
Future<ProviderDashboard> providerDashboard(Ref ref) async {
  await ref.watch(providerControllerProvider.future);
  return switch (await ref.read(providerRepositoryProvider).dashboard()) {
    ApiSuccess(:final data) => data,
    ApiFailure(:final problem) => throw problem,
  };
}

@riverpod
Future<List<ProviderPhoto>> providerPhotos(Ref ref) async => switch (await ref.read(providerRepositoryProvider).photos()) {
      ApiSuccess(:final data) => data,
      ApiFailure(:final problem) => throw problem,
    };

@riverpod
Future<PublicProvider?> publicProvider(Ref ref, String providerId) async {
  final area = await ref.watch(searchAreaProvider.future);
  final result = await ref.read(providerRepositoryProvider).publicProfile(providerId, latitude: area?.latitude, longitude: area?.longitude);
  return switch (result) {
    ApiSuccess(:final data) => data,
    ApiFailure(:final problem) when problem.status == 404 => null,
    ApiFailure(:final problem) => throw problem,
  };
}
