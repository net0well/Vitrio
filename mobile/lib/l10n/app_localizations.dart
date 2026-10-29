import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_pt.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'l10n/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations)!;
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[Locale('pt')];

  /// No description provided for @appTitle.
  ///
  /// In pt, this message translates to:
  /// **'Vitrio'**
  String get appTitle;

  /// No description provided for @welcomeTitle.
  ///
  /// In pt, this message translates to:
  /// **'Os profissionais da sua cidade, numa vitrine só.'**
  String get welcomeTitle;

  /// No description provided for @welcomeMessage.
  ///
  /// In pt, this message translates to:
  /// **'Encontre quem atende perto de você, peça orçamento sem compromisso e veja o que os vizinhos acharam.'**
  String get welcomeMessage;

  /// No description provided for @welcomeFindProfessional.
  ///
  /// In pt, this message translates to:
  /// **'Encontrar um profissional'**
  String get welcomeFindProfessional;

  /// No description provided for @welcomeOfferServices.
  ///
  /// In pt, this message translates to:
  /// **'Quero oferecer meus serviços'**
  String get welcomeOfferServices;

  /// No description provided for @tabHome.
  ///
  /// In pt, this message translates to:
  /// **'Início'**
  String get tabHome;

  /// No description provided for @tabSearch.
  ///
  /// In pt, this message translates to:
  /// **'Buscar'**
  String get tabSearch;

  /// No description provided for @tabOrders.
  ///
  /// In pt, this message translates to:
  /// **'Pedidos'**
  String get tabOrders;

  /// No description provided for @tabAccount.
  ///
  /// In pt, this message translates to:
  /// **'Conta'**
  String get tabAccount;

  /// No description provided for @homeTitle.
  ///
  /// In pt, this message translates to:
  /// **'Perto de você'**
  String get homeTitle;

  /// No description provided for @homeEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Logo você vai ver aqui os profissionais bem avaliados da sua cidade.'**
  String get homeEmpty;

  /// No description provided for @searchTitle.
  ///
  /// In pt, this message translates to:
  /// **'Buscar'**
  String get searchTitle;

  /// No description provided for @searchEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Em breve você vai poder buscar por serviço ou pelo nome do profissional.'**
  String get searchEmpty;

  /// No description provided for @ordersTitle.
  ///
  /// In pt, this message translates to:
  /// **'Pedidos'**
  String get ordersTitle;

  /// No description provided for @ordersEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Você ainda não fez nenhum pedido. Que tal começar pela busca?'**
  String get ordersEmpty;

  /// No description provided for @ordersEmptyAction.
  ///
  /// In pt, this message translates to:
  /// **'Buscar profissionais'**
  String get ordersEmptyAction;

  /// No description provided for @accountTitle.
  ///
  /// In pt, this message translates to:
  /// **'Conta'**
  String get accountTitle;

  /// No description provided for @accountEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Entre ou crie sua conta para pedir orçamentos e oferecer seus serviços.'**
  String get accountEmpty;

  /// No description provided for @authSignIn.
  ///
  /// In pt, this message translates to:
  /// **'Entrar'**
  String get authSignIn;

  /// No description provided for @authCreateAccount.
  ///
  /// In pt, this message translates to:
  /// **'Criar conta'**
  String get authCreateAccount;

  /// No description provided for @authEmail.
  ///
  /// In pt, this message translates to:
  /// **'E-mail'**
  String get authEmail;

  /// No description provided for @authPassword.
  ///
  /// In pt, this message translates to:
  /// **'Senha'**
  String get authPassword;

  /// No description provided for @authShowPassword.
  ///
  /// In pt, this message translates to:
  /// **'Mostrar senha'**
  String get authShowPassword;

  /// No description provided for @authHidePassword.
  ///
  /// In pt, this message translates to:
  /// **'Esconder senha'**
  String get authHidePassword;

  /// No description provided for @authForgotPassword.
  ///
  /// In pt, this message translates to:
  /// **'Esqueci a senha'**
  String get authForgotPassword;

  /// No description provided for @authName.
  ///
  /// In pt, this message translates to:
  /// **'Nome'**
  String get authName;

  /// No description provided for @authPasswordRule.
  ///
  /// In pt, this message translates to:
  /// **'Use pelo menos 10 caracteres. Uma frase com espaços vale.'**
  String get authPasswordRule;

  /// No description provided for @authAcceptTerms.
  ///
  /// In pt, this message translates to:
  /// **'Li e aceito os termos de uso e a política de privacidade.'**
  String get authAcceptTerms;

  /// No description provided for @authAcceptTermsRequired.
  ///
  /// In pt, this message translates to:
  /// **'Aceite os termos para continuar.'**
  String get authAcceptTermsRequired;

  /// No description provided for @authVerifyTitle.
  ///
  /// In pt, this message translates to:
  /// **'Confirme seu e-mail'**
  String get authVerifyTitle;

  /// No description provided for @authVerifyMessage.
  ///
  /// In pt, this message translates to:
  /// **'Enviamos um código de 6 números para {email}. Digite o código para terminar.'**
  String authVerifyMessage(String email);

  /// No description provided for @authCode.
  ///
  /// In pt, this message translates to:
  /// **'Código'**
  String get authCode;

  /// No description provided for @authConfirm.
  ///
  /// In pt, this message translates to:
  /// **'Confirmar'**
  String get authConfirm;

  /// No description provided for @authResendCode.
  ///
  /// In pt, this message translates to:
  /// **'Reenviar código'**
  String get authResendCode;

  /// No description provided for @authResendIn.
  ///
  /// In pt, this message translates to:
  /// **'Reenviar em {seconds} s'**
  String authResendIn(int seconds);

  /// No description provided for @authCodeResent.
  ///
  /// In pt, this message translates to:
  /// **'Enviamos um novo código.'**
  String get authCodeResent;

  /// No description provided for @authChangeEmail.
  ///
  /// In pt, this message translates to:
  /// **'Trocar e-mail'**
  String get authChangeEmail;

  /// No description provided for @authForgotTitle.
  ///
  /// In pt, this message translates to:
  /// **'Redefinir senha'**
  String get authForgotTitle;

  /// No description provided for @authForgotMessage.
  ///
  /// In pt, this message translates to:
  /// **'Digite o e-mail da sua conta. Vamos enviar um código para você criar uma senha nova.'**
  String get authForgotMessage;

  /// No description provided for @authSendCode.
  ///
  /// In pt, this message translates to:
  /// **'Enviar código'**
  String get authSendCode;

  /// No description provided for @authCodeSentIfExists.
  ///
  /// In pt, this message translates to:
  /// **'Se houver uma conta com este e-mail, enviamos um código para ele.'**
  String get authCodeSentIfExists;

  /// No description provided for @authNewPassword.
  ///
  /// In pt, this message translates to:
  /// **'Senha nova'**
  String get authNewPassword;

  /// No description provided for @authSaveNewPassword.
  ///
  /// In pt, this message translates to:
  /// **'Salvar senha nova'**
  String get authSaveNewPassword;

  /// No description provided for @errorConnection.
  ///
  /// In pt, this message translates to:
  /// **'Sem conexão. Confira a internet e tente de novo.'**
  String get errorConnection;

  /// No description provided for @errorGeneric.
  ///
  /// In pt, this message translates to:
  /// **'Algo deu errado. Tente de novo.'**
  String get errorGeneric;

  /// No description provided for @commonRetry.
  ///
  /// In pt, this message translates to:
  /// **'Tentar de novo'**
  String get commonRetry;

  /// No description provided for @commonCancel.
  ///
  /// In pt, this message translates to:
  /// **'Cancelar'**
  String get commonCancel;

  /// No description provided for @commonSave.
  ///
  /// In pt, this message translates to:
  /// **'Salvar'**
  String get commonSave;

  /// No description provided for @accountEditProfile.
  ///
  /// In pt, this message translates to:
  /// **'Editar dados'**
  String get accountEditProfile;

  /// No description provided for @accountChangePassword.
  ///
  /// In pt, this message translates to:
  /// **'Trocar senha'**
  String get accountChangePassword;

  /// No description provided for @accountDevices.
  ///
  /// In pt, this message translates to:
  /// **'Aparelhos conectados'**
  String get accountDevices;

  /// No description provided for @accountSignOut.
  ///
  /// In pt, this message translates to:
  /// **'Sair'**
  String get accountSignOut;

  /// No description provided for @accountDelete.
  ///
  /// In pt, this message translates to:
  /// **'Excluir conta'**
  String get accountDelete;

  /// No description provided for @accountDeleteTitle.
  ///
  /// In pt, this message translates to:
  /// **'Excluir sua conta?'**
  String get accountDeleteTitle;

  /// No description provided for @accountDeleteMessage.
  ///
  /// In pt, this message translates to:
  /// **'Seus dados pessoais serão apagados e você sai de todos os aparelhos. Isso não pode ser desfeito. Para confirmar, digite EXCLUIR.'**
  String get accountDeleteMessage;

  /// No description provided for @accountDeleteWord.
  ///
  /// In pt, this message translates to:
  /// **'EXCLUIR'**
  String get accountDeleteWord;

  /// No description provided for @accountDeleteConfirm.
  ///
  /// In pt, this message translates to:
  /// **'Excluir'**
  String get accountDeleteConfirm;

  /// No description provided for @profilePhone.
  ///
  /// In pt, this message translates to:
  /// **'Celular'**
  String get profilePhone;

  /// No description provided for @profilePhoneHint.
  ///
  /// In pt, this message translates to:
  /// **'(16) 99123-4567'**
  String get profilePhoneHint;

  /// No description provided for @profileSaved.
  ///
  /// In pt, this message translates to:
  /// **'Dados salvos.'**
  String get profileSaved;

  /// No description provided for @passwordCurrent.
  ///
  /// In pt, this message translates to:
  /// **'Senha atual'**
  String get passwordCurrent;

  /// No description provided for @passwordChanged.
  ///
  /// In pt, this message translates to:
  /// **'Senha trocada. Os outros aparelhos foram desconectados.'**
  String get passwordChanged;

  /// No description provided for @devicesThisDevice.
  ///
  /// In pt, this message translates to:
  /// **'Este aparelho'**
  String get devicesThisDevice;

  /// No description provided for @devicesLastSeen.
  ///
  /// In pt, this message translates to:
  /// **'Último acesso em {date}'**
  String devicesLastSeen(String date);

  /// No description provided for @devicesDisconnect.
  ///
  /// In pt, this message translates to:
  /// **'Desconectar'**
  String get devicesDisconnect;

  /// No description provided for @devicesSignOutAll.
  ///
  /// In pt, this message translates to:
  /// **'Sair de todos os aparelhos'**
  String get devicesSignOutAll;

  /// No description provided for @devicesOnlyThis.
  ///
  /// In pt, this message translates to:
  /// **'Só este aparelho está conectado.'**
  String get devicesOnlyThis;

  /// No description provided for @locationPermissionTitle.
  ///
  /// In pt, this message translates to:
  /// **'Para mostrar quem atende perto de você'**
  String get locationPermissionTitle;

  /// No description provided for @locationPermissionMessage.
  ///
  /// In pt, this message translates to:
  /// **'O Vitrio usa sua localização só enquanto o app está aberto e não guarda o histórico. Se preferir, escolha a cidade.'**
  String get locationPermissionMessage;

  /// No description provided for @locationUseMine.
  ///
  /// In pt, this message translates to:
  /// **'Usar minha localização'**
  String get locationUseMine;

  /// No description provided for @locationChooseCity.
  ///
  /// In pt, this message translates to:
  /// **'Escolher cidade'**
  String get locationChooseCity;

  /// No description provided for @locationSearchHint.
  ///
  /// In pt, this message translates to:
  /// **'Buscar cidade'**
  String get locationSearchHint;

  /// No description provided for @locationRecent.
  ///
  /// In pt, this message translates to:
  /// **'Recentes'**
  String get locationRecent;

  /// No description provided for @locationNoResults.
  ///
  /// In pt, this message translates to:
  /// **'Nenhuma cidade encontrada.'**
  String get locationNoResults;

  /// No description provided for @locationShowingCity.
  ///
  /// In pt, this message translates to:
  /// **'Mostrando profissionais de {city}'**
  String locationShowingCity(String city);

  /// No description provided for @locationDenied.
  ///
  /// In pt, this message translates to:
  /// **'Sem acesso à localização. Escolha a cidade.'**
  String get locationDenied;

  /// No description provided for @locationNotFound.
  ///
  /// In pt, this message translates to:
  /// **'Não encontramos sua cidade. Escolha na lista.'**
  String get locationNotFound;

  /// No description provided for @locationPillHint.
  ///
  /// In pt, this message translates to:
  /// **'Trocar cidade'**
  String get locationPillHint;
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['pt'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'pt':
      return AppLocalizationsPt();
  }

  throw FlutterError(
    'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
