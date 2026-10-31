import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/no_retry.dart';
import '../../location/data/location_providers.dart';
import '../../location/domain/location_mode.dart';
import '../../location/presentation/location_controller.dart';
import '../data/search_providers.dart';
import '../domain/home_feed.dart';
import '../domain/search_area.dart';

part 'search_queries.g.dart';

@Riverpod(keepAlive: true)
Future<SearchArea?> searchArea(Ref ref) async {
  final location = await ref.watch(locationControllerProvider.future);
  final city = location.city;
  if (location.mode == LocationMode.nearMe) {
    final position = await ref.read(deviceLocationProvider).currentPosition();
    if (position != null) {
      return SearchArea.nearMe(
        latitude: position.latitude,
        longitude: position.longitude,
        cityIbgeCode: city?.ibgeCode,
      );
    }
  }

  return city == null ? null : SearchArea.city(city.ibgeCode);
}

@Riverpod(retry: noRetry)
Future<HomeFeed?> homeFeed(Ref ref) async {
  final area = await ref.watch(searchAreaProvider.future);
  if (area == null) {
    return null;
  }

  return switch (await ref.read(searchRepositoryProvider).home(area)) {
    ApiSuccess(:final data) => data,
    ApiFailure(:final problem) => throw problem,
  };
}
