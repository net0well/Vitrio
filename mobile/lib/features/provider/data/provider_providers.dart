import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/network_providers.dart';
import '../../../core/storage/preferences.dart';
import 'app_mode_store.dart';
import 'http_photo_storage.dart';
import 'http_provider_repository.dart';
import 'photo_picker.dart';
import 'photo_storage.dart';
import 'provider_repository.dart';
import 'system_photo_picker.dart';

part 'provider_providers.g.dart';

@Riverpod(keepAlive: true)
ProviderRepository providerRepository(Ref ref) => HttpProviderRepository(ref.watch(apiClientProvider));

@Riverpod(keepAlive: true)
PhotoStorage photoStorage(Ref ref) => HttpPhotoStorage(ref.watch(httpClientProvider));

@Riverpod(keepAlive: true)
PhotoPicker photoPicker(Ref ref) => SystemPhotoPicker();

@Riverpod(keepAlive: true)
AppModeStore appModeStore(Ref ref) => AppModeStore(ref.watch(preferencesProvider));
