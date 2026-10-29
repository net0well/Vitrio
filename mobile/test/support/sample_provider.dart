import 'package:vitrio/features/auth/domain/me.dart';
import 'package:vitrio/features/provider/domain/provider_service.dart';
import 'package:vitrio/features/provider/domain/public_provider.dart';

import 'fake_provider_repository.dart';

const sampleProviderMe = Me(
  id: '0199a0c4-3f2e-7c1a-9d7e-2a4b6c8d0e1f',
  displayName: 'Carlos Souza',
  initials: 'CS',
  email: 'carlos@exemplo.com.br',
  phoneNumber: '+5516991234567',
  roles: ['customer', 'provider'],
  hasProviderProfile: true,
);

const samplePublicProvider = PublicProvider(
  id: '0199a0c4-0000-7000-8000-00000000c0de',
  displayName: 'Carlos Eletricista',
  headline: 'Eletricista',
  about: 'Eletricista há 12 anos, faço instalações e reparos residenciais com garantia.',
  city: jaboticabalCity,
  neighborhoods: ['Centro'],
  radiusKm: 15,
  categories: [],
  services: [ProviderService(title: 'Troca de chuveiro', priceFrom: 80)],
  availability: [],
  handlesEmergencies: false,
  meiVerified: true,
  availableToday: true,
  isNew: true,
  photos: [],
  shareUrl: 'https://vitrio.app/p/0199a0c4-0000-7000-8000-00000000c0de',
  whatsAppPhone: '+5516991234567',
);
