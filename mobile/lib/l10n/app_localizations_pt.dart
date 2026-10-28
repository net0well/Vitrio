// ignore: unused_import
import 'package:intl/intl.dart' as intl;

import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Portuguese (`pt`).
class AppLocalizationsPt extends AppLocalizations {
  AppLocalizationsPt([String locale = 'pt']) : super(locale);

  @override
  String get appTitle => 'Vitrio';

  @override
  String get welcomeTitle => 'Os profissionais da sua cidade, numa vitrine só.';

  @override
  String get welcomeMessage =>
      'Encontre quem atende perto de você, peça orçamento sem compromisso e veja o que os vizinhos acharam.';

  @override
  String get welcomeFindProfessional => 'Encontrar um profissional';

  @override
  String get welcomeOfferServices => 'Quero oferecer meus serviços';

  @override
  String get tabHome => 'Início';

  @override
  String get tabSearch => 'Buscar';

  @override
  String get tabOrders => 'Pedidos';

  @override
  String get tabAccount => 'Conta';

  @override
  String get homeTitle => 'Perto de você';

  @override
  String get homeEmpty =>
      'Logo você vai ver aqui os profissionais bem avaliados da sua cidade.';

  @override
  String get searchTitle => 'Buscar';

  @override
  String get searchEmpty =>
      'Em breve você vai poder buscar por serviço ou pelo nome do profissional.';

  @override
  String get ordersTitle => 'Pedidos';

  @override
  String get ordersEmpty =>
      'Você ainda não fez nenhum pedido. Que tal começar pela busca?';

  @override
  String get ordersEmptyAction => 'Buscar profissionais';

  @override
  String get accountTitle => 'Conta';

  @override
  String get accountEmpty =>
      'Em breve você vai poder entrar com Google ou Apple para pedir orçamentos e oferecer seus serviços.';
}
