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
  /// **'Encontre, contrate e avalie eletricistas, diaristas, pedreiros e muito mais, com a nota de quem já contratou.'**
  String get welcomeMessage;

  /// No description provided for @welcomeFindProfessional.
  ///
  /// In pt, this message translates to:
  /// **'Encontrar um profissional'**
  String get welcomeFindProfessional;

  /// No description provided for @brandFreeForEveryone.
  ///
  /// In pt, this message translates to:
  /// **'O Vitrio é grátis para todo mundo.'**
  String get brandFreeForEveryone;

  /// No description provided for @accountGuestTitle.
  ///
  /// In pt, this message translates to:
  /// **'Peça orçamentos e ofereça seus serviços'**
  String get accountGuestTitle;

  /// No description provided for @authSignInHeading.
  ///
  /// In pt, this message translates to:
  /// **'Que bom te ver de novo'**
  String get authSignInHeading;

  /// No description provided for @authSignInSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'Entre para pedir orçamentos e acompanhar seus pedidos.'**
  String get authSignInSubtitle;

  /// No description provided for @authSignUpHeading.
  ///
  /// In pt, this message translates to:
  /// **'Crie sua conta'**
  String get authSignUpHeading;

  /// No description provided for @authSignUpSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'É grátis e leva menos de um minuto.'**
  String get authSignUpSubtitle;

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
  /// **'Mostrando profissionais de {city}.'**
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

  /// No description provided for @tabDashboard.
  ///
  /// In pt, this message translates to:
  /// **'Painel'**
  String get tabDashboard;

  /// No description provided for @tabMyProfile.
  ///
  /// In pt, this message translates to:
  /// **'Meu perfil'**
  String get tabMyProfile;

  /// No description provided for @providerOfferServices.
  ///
  /// In pt, this message translates to:
  /// **'Quero oferecer meus serviços'**
  String get providerOfferServices;

  /// No description provided for @providerEnterMode.
  ///
  /// In pt, this message translates to:
  /// **'Entrar como prestador'**
  String get providerEnterMode;

  /// No description provided for @providerLeaveMode.
  ///
  /// In pt, this message translates to:
  /// **'Voltar para cliente'**
  String get providerLeaveMode;

  /// No description provided for @providerStepOf.
  ///
  /// In pt, this message translates to:
  /// **'Passo {step} de {total}'**
  String providerStepOf(int step, int total);

  /// No description provided for @providerStepAbout.
  ///
  /// In pt, this message translates to:
  /// **'Sobre você'**
  String get providerStepAbout;

  /// No description provided for @providerStepShowcase.
  ///
  /// In pt, this message translates to:
  /// **'Monte sua vitrine'**
  String get providerStepShowcase;

  /// No description provided for @providerStepPhotos.
  ///
  /// In pt, this message translates to:
  /// **'Fotos e preços'**
  String get providerStepPhotos;

  /// No description provided for @providerStepReview.
  ///
  /// In pt, this message translates to:
  /// **'Revisar e publicar'**
  String get providerStepReview;

  /// No description provided for @providerContinue.
  ///
  /// In pt, this message translates to:
  /// **'Continuar'**
  String get providerContinue;

  /// No description provided for @providerPublish.
  ///
  /// In pt, this message translates to:
  /// **'Publicar'**
  String get providerPublish;

  /// No description provided for @providerDisplayName.
  ///
  /// In pt, this message translates to:
  /// **'Nome que os clientes vão ver'**
  String get providerDisplayName;

  /// No description provided for @providerHeadline.
  ///
  /// In pt, this message translates to:
  /// **'Profissão'**
  String get providerHeadline;

  /// No description provided for @providerHeadlineHint.
  ///
  /// In pt, this message translates to:
  /// **'Ex.: Eletricista'**
  String get providerHeadlineHint;

  /// No description provided for @providerAbout.
  ///
  /// In pt, this message translates to:
  /// **'Sobre você e o seu trabalho'**
  String get providerAbout;

  /// No description provided for @providerAboutHint.
  ///
  /// In pt, this message translates to:
  /// **'Conte há quanto tempo trabalha, o que faz e o que te diferencia.'**
  String get providerAboutHint;

  /// No description provided for @providerYears.
  ///
  /// In pt, this message translates to:
  /// **'Anos de experiência'**
  String get providerYears;

  /// No description provided for @providerAvatarAdd.
  ///
  /// In pt, this message translates to:
  /// **'Adicionar foto de perfil'**
  String get providerAvatarAdd;

  /// No description provided for @providerAvatarChange.
  ///
  /// In pt, this message translates to:
  /// **'Trocar foto de perfil'**
  String get providerAvatarChange;

  /// No description provided for @providerCategories.
  ///
  /// In pt, this message translates to:
  /// **'O que você faz?'**
  String get providerCategories;

  /// No description provided for @providerCategoriesHint.
  ///
  /// In pt, this message translates to:
  /// **'Escolha até 3 categorias. A primeira é a principal.'**
  String get providerCategoriesHint;

  /// No description provided for @providerCategoriesRequired.
  ///
  /// In pt, this message translates to:
  /// **'Escolha ao menos uma categoria.'**
  String get providerCategoriesRequired;

  /// No description provided for @providerPrimary.
  ///
  /// In pt, this message translates to:
  /// **'Principal'**
  String get providerPrimary;

  /// No description provided for @providerServiceArea.
  ///
  /// In pt, this message translates to:
  /// **'Onde você atende?'**
  String get providerServiceArea;

  /// No description provided for @providerCep.
  ///
  /// In pt, this message translates to:
  /// **'CEP do endereço base'**
  String get providerCep;

  /// No description provided for @providerNumber.
  ///
  /// In pt, this message translates to:
  /// **'Número'**
  String get providerNumber;

  /// No description provided for @providerComplement.
  ///
  /// In pt, this message translates to:
  /// **'Complemento'**
  String get providerComplement;

  /// No description provided for @providerBasePrivate.
  ///
  /// In pt, this message translates to:
  /// **'Seu endereço não aparece para ninguém. Os clientes veem só a cidade, os bairros que você atende e a distância aproximada.'**
  String get providerBasePrivate;

  /// No description provided for @providerRadius.
  ///
  /// In pt, this message translates to:
  /// **'até {km} km'**
  String providerRadius(int km);

  /// No description provided for @providerNeighborhoods.
  ///
  /// In pt, this message translates to:
  /// **'Bairros atendidos'**
  String get providerNeighborhoods;

  /// No description provided for @providerNeighborhoodHint.
  ///
  /// In pt, this message translates to:
  /// **'Digite o bairro e toque em adicionar'**
  String get providerNeighborhoodHint;

  /// No description provided for @providerNeighborhoodAdd.
  ///
  /// In pt, this message translates to:
  /// **'Adicionar bairro'**
  String get providerNeighborhoodAdd;

  /// No description provided for @providerApproximate.
  ///
  /// In pt, this message translates to:
  /// **'Não achamos o ponto exato do seu CEP. Usamos o centro da cidade, então as distâncias ficam aproximadas.'**
  String get providerApproximate;

  /// No description provided for @providerContact.
  ///
  /// In pt, this message translates to:
  /// **'Contato'**
  String get providerContact;

  /// No description provided for @providerContactPhone.
  ///
  /// In pt, this message translates to:
  /// **'WhatsApp para contato'**
  String get providerContactPhone;

  /// No description provided for @providerWhatsApp.
  ///
  /// In pt, this message translates to:
  /// **'Mostrar o botão de WhatsApp no perfil'**
  String get providerWhatsApp;

  /// No description provided for @providerWhatsAppHint.
  ///
  /// In pt, this message translates to:
  /// **'Sem ele, o contato é só pelo pedido no app.'**
  String get providerWhatsAppHint;

  /// No description provided for @providerMei.
  ///
  /// In pt, this message translates to:
  /// **'Tem MEI? Ganhe o selo verificado'**
  String get providerMei;

  /// No description provided for @providerMeiHint.
  ///
  /// In pt, this message translates to:
  /// **'Consultamos seu CNPJ na Receita Federal.'**
  String get providerMeiHint;

  /// No description provided for @providerCnpj.
  ///
  /// In pt, this message translates to:
  /// **'CNPJ'**
  String get providerCnpj;

  /// No description provided for @providerVerify.
  ///
  /// In pt, this message translates to:
  /// **'Verificar'**
  String get providerVerify;

  /// No description provided for @providerMeiVerified.
  ///
  /// In pt, this message translates to:
  /// **'CNPJ ativo e optante pelo MEI. O selo já aparece no seu perfil.'**
  String get providerMeiVerified;

  /// No description provided for @providerMeiPending.
  ///
  /// In pt, this message translates to:
  /// **'Consulta pendente. Vamos tentar de novo nas próximas horas.'**
  String get providerMeiPending;

  /// No description provided for @providerMeiLost.
  ///
  /// In pt, this message translates to:
  /// **'O selo MEI saiu na última revalidação.'**
  String get providerMeiLost;

  /// No description provided for @providerPhotos.
  ///
  /// In pt, this message translates to:
  /// **'Fotos dos seus trabalhos'**
  String get providerPhotos;

  /// No description provided for @providerPhotosHint.
  ///
  /// In pt, this message translates to:
  /// **'Até 30 fotos. Elas aparecem no perfil depois de processadas.'**
  String get providerPhotosHint;

  /// No description provided for @providerPhotoAdd.
  ///
  /// In pt, this message translates to:
  /// **'Adicionar foto'**
  String get providerPhotoAdd;

  /// No description provided for @providerPhotoProcessing.
  ///
  /// In pt, this message translates to:
  /// **'Processando'**
  String get providerPhotoProcessing;

  /// No description provided for @providerPhotoRejected.
  ///
  /// In pt, this message translates to:
  /// **'Recusada'**
  String get providerPhotoRejected;

  /// No description provided for @providerPhotoUploadFailed.
  ///
  /// In pt, this message translates to:
  /// **'Não conseguimos enviar a foto. Tente de novo.'**
  String get providerPhotoUploadFailed;

  /// No description provided for @providerPhotoDelete.
  ///
  /// In pt, this message translates to:
  /// **'Apagar foto'**
  String get providerPhotoDelete;

  /// No description provided for @providerServices.
  ///
  /// In pt, this message translates to:
  /// **'Serviços e preços'**
  String get providerServices;

  /// No description provided for @providerServicesEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Cadastre ao menos um serviço.'**
  String get providerServicesEmpty;

  /// No description provided for @providerServiceAdd.
  ///
  /// In pt, this message translates to:
  /// **'Adicionar serviço'**
  String get providerServiceAdd;

  /// No description provided for @providerServiceTitle.
  ///
  /// In pt, this message translates to:
  /// **'Nome do serviço'**
  String get providerServiceTitle;

  /// No description provided for @providerServiceDescription.
  ///
  /// In pt, this message translates to:
  /// **'Descrição (opcional)'**
  String get providerServiceDescription;

  /// No description provided for @providerServicePrice.
  ///
  /// In pt, this message translates to:
  /// **'A partir de'**
  String get providerServicePrice;

  /// No description provided for @providerServiceOnQuote.
  ///
  /// In pt, this message translates to:
  /// **'Sob orçamento'**
  String get providerServiceOnQuote;

  /// No description provided for @providerServiceUnit.
  ///
  /// In pt, this message translates to:
  /// **'Cobrado por'**
  String get providerServiceUnit;

  /// No description provided for @providerUnitPerService.
  ///
  /// In pt, this message translates to:
  /// **'serviço'**
  String get providerUnitPerService;

  /// No description provided for @providerUnitPerHour.
  ///
  /// In pt, this message translates to:
  /// **'hora'**
  String get providerUnitPerHour;

  /// No description provided for @providerUnitPerDay.
  ///
  /// In pt, this message translates to:
  /// **'dia'**
  String get providerUnitPerDay;

  /// No description provided for @providerUnitPerSquareMeter.
  ///
  /// In pt, this message translates to:
  /// **'m²'**
  String get providerUnitPerSquareMeter;

  /// No description provided for @providerUnitPerUnit.
  ///
  /// In pt, this message translates to:
  /// **'unidade'**
  String get providerUnitPerUnit;

  /// No description provided for @providerPriceFrom.
  ///
  /// In pt, this message translates to:
  /// **'a partir de {price}/{unit}'**
  String providerPriceFrom(String price, String unit);

  /// No description provided for @providerServiceRemove.
  ///
  /// In pt, this message translates to:
  /// **'Remover serviço'**
  String get providerServiceRemove;

  /// No description provided for @providerReviewMissing.
  ///
  /// In pt, this message translates to:
  /// **'Para publicar, falta:'**
  String get providerReviewMissing;

  /// No description provided for @providerReviewReady.
  ///
  /// In pt, this message translates to:
  /// **'Tudo pronto para publicar.'**
  String get providerReviewReady;

  /// No description provided for @providerMissingAbout.
  ///
  /// In pt, this message translates to:
  /// **'o texto sobre você'**
  String get providerMissingAbout;

  /// No description provided for @providerMissingCategories.
  ///
  /// In pt, this message translates to:
  /// **'uma categoria'**
  String get providerMissingCategories;

  /// No description provided for @providerMissingServiceArea.
  ///
  /// In pt, this message translates to:
  /// **'a área de atendimento'**
  String get providerMissingServiceArea;

  /// No description provided for @providerMissingServices.
  ///
  /// In pt, this message translates to:
  /// **'um serviço'**
  String get providerMissingServices;

  /// No description provided for @providerMissingContactPhone.
  ///
  /// In pt, this message translates to:
  /// **'o WhatsApp para contato'**
  String get providerMissingContactPhone;

  /// No description provided for @providerPublished.
  ///
  /// In pt, this message translates to:
  /// **'Vitrine publicada.'**
  String get providerPublished;

  /// No description provided for @providerStatusDraft.
  ///
  /// In pt, this message translates to:
  /// **'Rascunho'**
  String get providerStatusDraft;

  /// No description provided for @providerStatusPublished.
  ///
  /// In pt, this message translates to:
  /// **'Publicada'**
  String get providerStatusPublished;

  /// No description provided for @providerStatusPaused.
  ///
  /// In pt, this message translates to:
  /// **'Pausada'**
  String get providerStatusPaused;

  /// No description provided for @providerStatusSuspended.
  ///
  /// In pt, this message translates to:
  /// **'Suspensa'**
  String get providerStatusSuspended;

  /// No description provided for @providerAvailableToday.
  ///
  /// In pt, this message translates to:
  /// **'Disponível hoje'**
  String get providerAvailableToday;

  /// No description provided for @providerAvailableTodayHint.
  ///
  /// In pt, this message translates to:
  /// **'Aparece no seu perfil até a meia-noite.'**
  String get providerAvailableTodayHint;

  /// No description provided for @providerCompleteness.
  ///
  /// In pt, this message translates to:
  /// **'Perfil {percent}% completo'**
  String providerCompleteness(int percent);

  /// No description provided for @providerNextStep.
  ///
  /// In pt, this message translates to:
  /// **'Próximo passo'**
  String get providerNextStep;

  /// No description provided for @providerNextPublish.
  ///
  /// In pt, this message translates to:
  /// **'Publicar a vitrine'**
  String get providerNextPublish;

  /// No description provided for @providerNextShare.
  ///
  /// In pt, this message translates to:
  /// **'Compartilhar o perfil'**
  String get providerNextShare;

  /// No description provided for @providerNextAvailability.
  ///
  /// In pt, this message translates to:
  /// **'Informar os dias e horários'**
  String get providerNextAvailability;

  /// No description provided for @providerViewPublic.
  ///
  /// In pt, this message translates to:
  /// **'Ver como os clientes veem'**
  String get providerViewPublic;

  /// No description provided for @providerShare.
  ///
  /// In pt, this message translates to:
  /// **'Compartilhar perfil'**
  String get providerShare;

  /// No description provided for @providerShareText.
  ///
  /// In pt, this message translates to:
  /// **'Veja meu perfil no Vitrio: {url}'**
  String providerShareText(String url);

  /// No description provided for @providerAvailability.
  ///
  /// In pt, this message translates to:
  /// **'Dias e horários'**
  String get providerAvailability;

  /// No description provided for @providerEmergencies.
  ///
  /// In pt, this message translates to:
  /// **'Atende emergências'**
  String get providerEmergencies;

  /// No description provided for @providerPause.
  ///
  /// In pt, this message translates to:
  /// **'Pausar a vitrine'**
  String get providerPause;

  /// No description provided for @providerResume.
  ///
  /// In pt, this message translates to:
  /// **'Publicar de novo'**
  String get providerResume;

  /// No description provided for @providerPausedHint.
  ///
  /// In pt, this message translates to:
  /// **'Vitrine pausada. Ela não aparece para os clientes.'**
  String get providerPausedHint;

  /// No description provided for @providerDayMonday.
  ///
  /// In pt, this message translates to:
  /// **'Segunda'**
  String get providerDayMonday;

  /// No description provided for @providerDayTuesday.
  ///
  /// In pt, this message translates to:
  /// **'Terça'**
  String get providerDayTuesday;

  /// No description provided for @providerDayWednesday.
  ///
  /// In pt, this message translates to:
  /// **'Quarta'**
  String get providerDayWednesday;

  /// No description provided for @providerDayThursday.
  ///
  /// In pt, this message translates to:
  /// **'Quinta'**
  String get providerDayThursday;

  /// No description provided for @providerDayFriday.
  ///
  /// In pt, this message translates to:
  /// **'Sexta'**
  String get providerDayFriday;

  /// No description provided for @providerDaySaturday.
  ///
  /// In pt, this message translates to:
  /// **'Sábado'**
  String get providerDaySaturday;

  /// No description provided for @providerDaySunday.
  ///
  /// In pt, this message translates to:
  /// **'Domingo'**
  String get providerDaySunday;

  /// No description provided for @providerFrom.
  ///
  /// In pt, this message translates to:
  /// **'Das'**
  String get providerFrom;

  /// No description provided for @providerTo.
  ///
  /// In pt, this message translates to:
  /// **'às'**
  String get providerTo;

  /// No description provided for @providerBadgeMei.
  ///
  /// In pt, this message translates to:
  /// **'MEI verificado'**
  String get providerBadgeMei;

  /// No description provided for @providerBadgeToday.
  ///
  /// In pt, this message translates to:
  /// **'Atende hoje'**
  String get providerBadgeToday;

  /// No description provided for @providerNew.
  ///
  /// In pt, this message translates to:
  /// **'Novo no Vitrio'**
  String get providerNew;

  /// No description provided for @providerRating.
  ///
  /// In pt, this message translates to:
  /// **'Nota {score} ({count} avaliações)'**
  String providerRating(String score, int count);

  /// No description provided for @providerServes.
  ///
  /// In pt, this message translates to:
  /// **'{city} · atende até {km} km'**
  String providerServes(String city, int km);

  /// No description provided for @providerYearsExperience.
  ///
  /// In pt, this message translates to:
  /// **'{years} anos de experiência'**
  String providerYearsExperience(int years);

  /// No description provided for @providerWhatsAppAction.
  ///
  /// In pt, this message translates to:
  /// **'WhatsApp'**
  String get providerWhatsAppAction;

  /// No description provided for @providerNotFound.
  ///
  /// In pt, this message translates to:
  /// **'Este perfil não está disponível.'**
  String get providerNotFound;

  /// No description provided for @providerAboutTitle.
  ///
  /// In pt, this message translates to:
  /// **'Sobre'**
  String get providerAboutTitle;

  /// No description provided for @providerWorks.
  ///
  /// In pt, this message translates to:
  /// **'Trabalhos'**
  String get providerWorks;

  /// No description provided for @providerHours.
  ///
  /// In pt, this message translates to:
  /// **'Horários'**
  String get providerHours;

  /// No description provided for @authShow.
  ///
  /// In pt, this message translates to:
  /// **'Mostrar'**
  String get authShow;

  /// No description provided for @authHide.
  ///
  /// In pt, this message translates to:
  /// **'Esconder'**
  String get authHide;

  /// No description provided for @providerOfferServicesHint.
  ///
  /// In pt, this message translates to:
  /// **'Monte sua vitrine e receba pedidos de clientes da sua cidade.'**
  String get providerOfferServicesHint;

  /// No description provided for @welcomeHaveAccount.
  ///
  /// In pt, this message translates to:
  /// **'Já tem conta?'**
  String get welcomeHaveAccount;

  /// No description provided for @welcomeNearYou.
  ///
  /// In pt, this message translates to:
  /// **'Perto\nde você'**
  String get welcomeNearYou;

  /// No description provided for @welcomeSampleAnaName.
  ///
  /// In pt, this message translates to:
  /// **'Ana Paula'**
  String get welcomeSampleAnaName;

  /// No description provided for @welcomeSampleAnaRole.
  ///
  /// In pt, this message translates to:
  /// **'Diarista'**
  String get welcomeSampleAnaRole;

  /// No description provided for @welcomeSampleAnaRating.
  ///
  /// In pt, this message translates to:
  /// **'5,0'**
  String get welcomeSampleAnaRating;

  /// No description provided for @welcomeSampleJoseName.
  ///
  /// In pt, this message translates to:
  /// **'José Lima'**
  String get welcomeSampleJoseName;

  /// No description provided for @welcomeSampleJoseRole.
  ///
  /// In pt, this message translates to:
  /// **'Pedreiro'**
  String get welcomeSampleJoseRole;

  /// No description provided for @welcomeSampleJoseRating.
  ///
  /// In pt, this message translates to:
  /// **'4,8'**
  String get welcomeSampleJoseRating;

  /// No description provided for @welcomeSampleCarlosName.
  ///
  /// In pt, this message translates to:
  /// **'Carlos Menezes'**
  String get welcomeSampleCarlosName;

  /// No description provided for @welcomeSampleCarlosRole.
  ///
  /// In pt, this message translates to:
  /// **'Eletricista · Vila Serra'**
  String get welcomeSampleCarlosRole;

  /// No description provided for @welcomeSampleCarlosRating.
  ///
  /// In pt, this message translates to:
  /// **'4,9'**
  String get welcomeSampleCarlosRating;

  /// No description provided for @welcomeSampleCarlosMeta.
  ///
  /// In pt, this message translates to:
  /// **'(127 avaliações) · 1,2 km'**
  String get welcomeSampleCarlosMeta;

  /// No description provided for @notificationsTitle.
  ///
  /// In pt, this message translates to:
  /// **'Notificações'**
  String get notificationsTitle;

  /// No description provided for @notificationsEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Você ainda não tem notificações.'**
  String get notificationsEmpty;

  /// No description provided for @homeGreeting.
  ///
  /// In pt, this message translates to:
  /// **'Olá, {name}'**
  String homeGreeting(String name);

  /// No description provided for @homeQuestion.
  ///
  /// In pt, this message translates to:
  /// **'Do que você precisa hoje?'**
  String get homeQuestion;

  /// No description provided for @searchFieldLabel.
  ///
  /// In pt, this message translates to:
  /// **'Buscar serviço'**
  String get searchFieldLabel;

  /// No description provided for @searchPlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'Eletricista, diarista, pintor…'**
  String get searchPlaceholder;

  /// No description provided for @homeHeroTag.
  ///
  /// In pt, this message translates to:
  /// **'Disponíveis agora'**
  String get homeHeroTag;

  /// No description provided for @homeHeroTitle.
  ///
  /// In pt, this message translates to:
  /// **'Imprevisto em casa?'**
  String get homeHeroTitle;

  /// No description provided for @homeHeroText.
  ///
  /// In pt, this message translates to:
  /// **'Veja quem atende hoje perto de você, com nota e preço.'**
  String get homeHeroText;

  /// No description provided for @homeHeroAction.
  ///
  /// In pt, this message translates to:
  /// **'Ver disponíveis'**
  String get homeHeroAction;

  /// No description provided for @homeSignInTitle.
  ///
  /// In pt, this message translates to:
  /// **'O Vitrio funciona melhor com sua conta'**
  String get homeSignInTitle;

  /// No description provided for @homeSignInText.
  ///
  /// In pt, this message translates to:
  /// **'Entre para pedir orçamentos, salvar favoritos e acompanhar seus pedidos.'**
  String get homeSignInText;

  /// No description provided for @homeCategories.
  ///
  /// In pt, this message translates to:
  /// **'Categorias'**
  String get homeCategories;

  /// No description provided for @homeSeeAll.
  ///
  /// In pt, this message translates to:
  /// **'Ver todas'**
  String get homeSeeAll;

  /// No description provided for @homeMore.
  ///
  /// In pt, this message translates to:
  /// **'Mais'**
  String get homeMore;

  /// No description provided for @tabAgenda.
  ///
  /// In pt, this message translates to:
  /// **'Agenda'**
  String get tabAgenda;

  /// No description provided for @providerNewOrders.
  ///
  /// In pt, this message translates to:
  /// **'Novos pedidos'**
  String get providerNewOrders;

  /// No description provided for @providerNoOrders.
  ///
  /// In pt, this message translates to:
  /// **'Nenhum pedido novo por enquanto.'**
  String get providerNoOrders;

  /// No description provided for @providerNoOrdersShare.
  ///
  /// In pt, this message translates to:
  /// **'Compartilhe seu perfil para chegar aos primeiros clientes.'**
  String get providerNoOrdersShare;

  /// No description provided for @providerNoOrdersPublish.
  ///
  /// In pt, this message translates to:
  /// **'Publique sua vitrine para começar a receber pedidos.'**
  String get providerNoOrdersPublish;

  /// No description provided for @providerAvailableTitle.
  ///
  /// In pt, this message translates to:
  /// **'Disponível para novos pedidos'**
  String get providerAvailableTitle;

  /// No description provided for @providerAvailableSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'Você aparece em “Atende hoje”'**
  String get providerAvailableSubtitle;

  /// No description provided for @providerUnavailableTitle.
  ///
  /// In pt, this message translates to:
  /// **'Pausado para novos pedidos'**
  String get providerUnavailableTitle;

  /// No description provided for @providerUnavailableSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'Clientes ainda veem seu perfil'**
  String get providerUnavailableSubtitle;

  /// No description provided for @providerAvailableNeedsPublish.
  ///
  /// In pt, this message translates to:
  /// **'Fica disponível depois que a vitrine for publicada.'**
  String get providerAvailableNeedsPublish;

  /// No description provided for @providerMetricNewOrders.
  ///
  /// In pt, this message translates to:
  /// **'pedidos novos'**
  String get providerMetricNewOrders;

  /// No description provided for @providerMetricRating.
  ///
  /// In pt, this message translates to:
  /// **'sua nota'**
  String get providerMetricRating;

  /// No description provided for @providerMetricServicesMonth.
  ///
  /// In pt, this message translates to:
  /// **'serviços no mês'**
  String get providerMetricServicesMonth;

  /// No description provided for @providerProfileAlmostReady.
  ///
  /// In pt, this message translates to:
  /// **'Seu perfil está quase pronto'**
  String get providerProfileAlmostReady;

  /// No description provided for @providerProfileComplete.
  ///
  /// In pt, this message translates to:
  /// **'Seu perfil está completo'**
  String get providerProfileComplete;

  /// No description provided for @providerPercent.
  ///
  /// In pt, this message translates to:
  /// **'{percent}%'**
  String providerPercent(int percent);

  /// No description provided for @providerCompleteProfile.
  ///
  /// In pt, this message translates to:
  /// **'Completar perfil'**
  String get providerCompleteProfile;

  /// No description provided for @providerHintAbout.
  ///
  /// In pt, this message translates to:
  /// **'Conte sobre você e o seu trabalho para os clientes te conhecerem.'**
  String get providerHintAbout;

  /// No description provided for @providerHintCategories.
  ///
  /// In pt, this message translates to:
  /// **'Escolha as categorias do que você faz.'**
  String get providerHintCategories;

  /// No description provided for @providerHintServiceArea.
  ///
  /// In pt, this message translates to:
  /// **'Informe onde você atende.'**
  String get providerHintServiceArea;

  /// No description provided for @providerHintContactPhone.
  ///
  /// In pt, this message translates to:
  /// **'Informe o WhatsApp para contato.'**
  String get providerHintContactPhone;

  /// No description provided for @providerHintServices.
  ///
  /// In pt, this message translates to:
  /// **'Cadastre seus serviços e preços.'**
  String get providerHintServices;

  /// No description provided for @providerHintPublish.
  ///
  /// In pt, this message translates to:
  /// **'Falta publicar a vitrine para aparecer nas buscas.'**
  String get providerHintPublish;

  /// No description provided for @providerHintAvailability.
  ///
  /// In pt, this message translates to:
  /// **'Informe seus dias e horários para os clientes saberem quando você atende.'**
  String get providerHintAvailability;

  /// No description provided for @providerHintShare.
  ///
  /// In pt, this message translates to:
  /// **'Compartilhe seu perfil para receber os primeiros pedidos.'**
  String get providerHintShare;

  /// No description provided for @providerShareShort.
  ///
  /// In pt, this message translates to:
  /// **'Compartilhar'**
  String get providerShareShort;

  /// No description provided for @providerYourShowcase.
  ///
  /// In pt, this message translates to:
  /// **'Sua vitrine'**
  String get providerYourShowcase;

  /// No description provided for @providerSkip.
  ///
  /// In pt, this message translates to:
  /// **'Pular'**
  String get providerSkip;

  /// No description provided for @providerAboutSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'Seu nome, sua profissão e um pouco da sua história.'**
  String get providerAboutSubtitle;

  /// No description provided for @providerShowcaseSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'Escolha o que você faz e onde atende. Dá para mudar quando quiser.'**
  String get providerShowcaseSubtitle;

  /// No description provided for @providerPhotosSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'Mostre trabalhos que você já fez e quanto cobra pelos serviços.'**
  String get providerPhotosSubtitle;

  /// No description provided for @providerReviewSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'Confira como os clientes vão ver sua vitrine antes de publicar.'**
  String get providerReviewSubtitle;

  /// No description provided for @providerNextAfterAbout.
  ///
  /// In pt, this message translates to:
  /// **'Próximo: o que você faz e onde atende'**
  String get providerNextAfterAbout;

  /// No description provided for @providerNextAfterShowcase.
  ///
  /// In pt, this message translates to:
  /// **'Próximo: fotos dos seus trabalhos e preços'**
  String get providerNextAfterShowcase;

  /// No description provided for @providerNextAfterPhotos.
  ///
  /// In pt, this message translates to:
  /// **'Próximo: revisar e publicar'**
  String get providerNextAfterPhotos;

  /// No description provided for @providerCepPlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'00000-000'**
  String get providerCepPlaceholder;

  /// No description provided for @providerRadiusLabel.
  ///
  /// In pt, this message translates to:
  /// **'Raio de atendimento'**
  String get providerRadiusLabel;

  /// No description provided for @providerContactPhoneHint.
  ///
  /// In pt, this message translates to:
  /// **'Usado no botão de WhatsApp do seu perfil.'**
  String get providerContactPhoneHint;

  /// No description provided for @providerPhonePlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'(16) 9 0000-0000'**
  String get providerPhonePlaceholder;

  /// No description provided for @providerCnpjPlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'00.000.000/0001-00'**
  String get providerCnpjPlaceholder;

  /// No description provided for @providerServiceEdit.
  ///
  /// In pt, this message translates to:
  /// **'Editar serviço'**
  String get providerServiceEdit;

  /// No description provided for @providerServiceOnQuoteHint.
  ///
  /// In pt, this message translates to:
  /// **'O preço é combinado depois de ver o serviço.'**
  String get providerServiceOnQuoteHint;

  /// No description provided for @providerFromLabel.
  ///
  /// In pt, this message translates to:
  /// **'a partir de'**
  String get providerFromLabel;

  /// No description provided for @providerFromCapital.
  ///
  /// In pt, this message translates to:
  /// **'A partir de'**
  String get providerFromCapital;

  /// No description provided for @providerPriceLabel.
  ///
  /// In pt, this message translates to:
  /// **'Preço'**
  String get providerPriceLabel;

  /// No description provided for @providerSeeProfile.
  ///
  /// In pt, this message translates to:
  /// **'Ver perfil'**
  String get providerSeeProfile;

  /// No description provided for @providerPreviewTitle.
  ///
  /// In pt, this message translates to:
  /// **'Como você aparece na busca'**
  String get providerPreviewTitle;

  /// No description provided for @providerMetricReviews.
  ///
  /// In pt, this message translates to:
  /// **'{count} avaliações'**
  String providerMetricReviews(int count);

  /// No description provided for @providerDistance.
  ///
  /// In pt, this message translates to:
  /// **'{km} km'**
  String providerDistance(String km);

  /// No description provided for @providerMetricDistance.
  ///
  /// In pt, this message translates to:
  /// **'de você'**
  String get providerMetricDistance;

  /// No description provided for @providerWhereWhen.
  ///
  /// In pt, this message translates to:
  /// **'Onde e quando atende'**
  String get providerWhereWhen;

  /// No description provided for @providerRadiusFrom.
  ///
  /// In pt, this message translates to:
  /// **'Até {km} km de {place}'**
  String providerRadiusFrom(int km, String place);

  /// No description provided for @providerMoreNeighborhoods.
  ///
  /// In pt, this message translates to:
  /// **'+ {count} bairros'**
  String providerMoreNeighborhoods(int count);

  /// No description provided for @providerNoPhotos.
  ///
  /// In pt, this message translates to:
  /// **'Ainda não há fotos de trabalhos.'**
  String get providerNoPhotos;

  /// No description provided for @providerReviews.
  ///
  /// In pt, this message translates to:
  /// **'Avaliações'**
  String get providerReviews;

  /// No description provided for @providerNoReviews.
  ///
  /// In pt, this message translates to:
  /// **'Ainda não há avaliações. Elas aparecem aqui depois dos primeiros serviços contratados pelo app.'**
  String get providerNoReviews;

  /// No description provided for @providerRatingOf.
  ///
  /// In pt, this message translates to:
  /// **'Nota {score} de 5'**
  String providerRatingOf(String score);

  /// No description provided for @providerDayShortMonday.
  ///
  /// In pt, this message translates to:
  /// **'seg'**
  String get providerDayShortMonday;

  /// No description provided for @providerDayShortTuesday.
  ///
  /// In pt, this message translates to:
  /// **'ter'**
  String get providerDayShortTuesday;

  /// No description provided for @providerDayShortWednesday.
  ///
  /// In pt, this message translates to:
  /// **'qua'**
  String get providerDayShortWednesday;

  /// No description provided for @providerDayShortThursday.
  ///
  /// In pt, this message translates to:
  /// **'qui'**
  String get providerDayShortThursday;

  /// No description provided for @providerDayShortFriday.
  ///
  /// In pt, this message translates to:
  /// **'sex'**
  String get providerDayShortFriday;

  /// No description provided for @providerDayShortSaturday.
  ///
  /// In pt, this message translates to:
  /// **'sáb'**
  String get providerDayShortSaturday;

  /// No description provided for @providerDayShortSunday.
  ///
  /// In pt, this message translates to:
  /// **'dom'**
  String get providerDayShortSunday;

  /// No description provided for @providerDaysRange.
  ///
  /// In pt, this message translates to:
  /// **'{first} a {last}'**
  String providerDaysRange(String first, String last);

  /// No description provided for @providerHoursRange.
  ///
  /// In pt, this message translates to:
  /// **'{start} às {end}'**
  String providerHoursRange(String start, String end);

  /// No description provided for @providerEmergenciesShort.
  ///
  /// In pt, this message translates to:
  /// **'atende emergências'**
  String get providerEmergenciesShort;

  /// No description provided for @providerCepNotFound.
  ///
  /// In pt, this message translates to:
  /// **'Não encontramos este CEP. Confira os 8 números do endereço base.'**
  String get providerCepNotFound;

  /// No description provided for @searchActivePlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'O que você procura?'**
  String get searchActivePlaceholder;

  /// No description provided for @searchClear.
  ///
  /// In pt, this message translates to:
  /// **'Limpar busca'**
  String get searchClear;

  /// No description provided for @searchFilters.
  ///
  /// In pt, this message translates to:
  /// **'Filtros'**
  String get searchFilters;

  /// No description provided for @searchNearMe.
  ///
  /// In pt, this message translates to:
  /// **'Perto de mim'**
  String get searchNearMe;

  /// No description provided for @searchTopRated.
  ///
  /// In pt, this message translates to:
  /// **'Nota 4,5+'**
  String get searchTopRated;

  /// No description provided for @searchLowestPrice.
  ///
  /// In pt, this message translates to:
  /// **'Menor preço'**
  String get searchLowestPrice;

  /// No description provided for @searchProfessionalOne.
  ///
  /// In pt, this message translates to:
  /// **'profissional'**
  String get searchProfessionalOne;

  /// No description provided for @searchProfessionalMany.
  ///
  /// In pt, this message translates to:
  /// **'profissionais'**
  String get searchProfessionalMany;

  /// No description provided for @searchCountNearMe.
  ///
  /// In pt, this message translates to:
  /// **'{count} {professionals} perto de você'**
  String searchCountNearMe(String count, String professionals);

  /// No description provided for @searchCountInCity.
  ///
  /// In pt, this message translates to:
  /// **'{count} {professionals} em {city}'**
  String searchCountInCity(String count, String professionals, String city);

  /// No description provided for @searchMoreThanNearMe.
  ///
  /// In pt, this message translates to:
  /// **'Mais de {count} {professionals} perto de você'**
  String searchMoreThanNearMe(String count, String professionals);

  /// No description provided for @searchMoreThanInCity.
  ///
  /// In pt, this message translates to:
  /// **'Mais de {count} {professionals} em {city}'**
  String searchMoreThanInCity(String count, String professionals, String city);

  /// No description provided for @searchSortTitle.
  ///
  /// In pt, this message translates to:
  /// **'Ordenar por'**
  String get searchSortTitle;

  /// No description provided for @searchSortRelevance.
  ///
  /// In pt, this message translates to:
  /// **'Relevância'**
  String get searchSortRelevance;

  /// No description provided for @searchSortDistance.
  ///
  /// In pt, this message translates to:
  /// **'Mais perto'**
  String get searchSortDistance;

  /// No description provided for @searchSortRating.
  ///
  /// In pt, this message translates to:
  /// **'Melhor avaliado'**
  String get searchSortRating;

  /// No description provided for @searchSortPrice.
  ///
  /// In pt, this message translates to:
  /// **'Menor preço'**
  String get searchSortPrice;

  /// No description provided for @searchPriceFrom.
  ///
  /// In pt, this message translates to:
  /// **'A partir de'**
  String get searchPriceFrom;

  /// No description provided for @searchSeeProfile.
  ///
  /// In pt, this message translates to:
  /// **'Ver perfil'**
  String get searchSeeProfile;

  /// No description provided for @searchReviewCount.
  ///
  /// In pt, this message translates to:
  /// **'({count})'**
  String searchReviewCount(String count);

  /// No description provided for @searchNoResults.
  ///
  /// In pt, this message translates to:
  /// **'Nenhum profissional encontrado com esses filtros.'**
  String get searchNoResults;

  /// No description provided for @searchClearFilters.
  ///
  /// In pt, this message translates to:
  /// **'Limpar filtros'**
  String get searchClearFilters;

  /// No description provided for @searchError.
  ///
  /// In pt, this message translates to:
  /// **'Não conseguimos carregar os profissionais. Verifique a conexão e tente de novo.'**
  String get searchError;

  /// No description provided for @searchChooseCity.
  ///
  /// In pt, this message translates to:
  /// **'Escolha uma cidade para ver os profissionais.'**
  String get searchChooseCity;

  /// No description provided for @searchFilterDistance.
  ///
  /// In pt, this message translates to:
  /// **'Distância máxima'**
  String get searchFilterDistance;

  /// No description provided for @searchFilterDistanceValue.
  ///
  /// In pt, this message translates to:
  /// **'Até {km} km'**
  String searchFilterDistanceValue(String km);

  /// No description provided for @searchFilterRating.
  ///
  /// In pt, this message translates to:
  /// **'Nota mínima'**
  String get searchFilterRating;

  /// No description provided for @searchFilterAny.
  ///
  /// In pt, this message translates to:
  /// **'Qualquer'**
  String get searchFilterAny;

  /// No description provided for @searchFilterRatingValue.
  ///
  /// In pt, this message translates to:
  /// **'{rating}+'**
  String searchFilterRatingValue(String rating);

  /// No description provided for @searchFilterMeiHint.
  ///
  /// In pt, this message translates to:
  /// **'CNPJ ativo e optante pelo MEI'**
  String get searchFilterMeiHint;

  /// No description provided for @searchFilterTodayHint.
  ///
  /// In pt, this message translates to:
  /// **'Quem ligou “Disponível hoje”'**
  String get searchFilterTodayHint;

  /// No description provided for @searchFilterPrice.
  ///
  /// In pt, this message translates to:
  /// **'Preço a partir de'**
  String get searchFilterPrice;

  /// No description provided for @searchFilterPriceValue.
  ///
  /// In pt, this message translates to:
  /// **'Até {price}'**
  String searchFilterPriceValue(String price);

  /// No description provided for @searchFilterCity.
  ///
  /// In pt, this message translates to:
  /// **'Cidade'**
  String get searchFilterCity;

  /// No description provided for @searchFilterApply.
  ///
  /// In pt, this message translates to:
  /// **'Ver resultados'**
  String get searchFilterApply;

  /// No description provided for @searchFilterReset.
  ///
  /// In pt, this message translates to:
  /// **'Limpar'**
  String get searchFilterReset;

  /// No description provided for @homeTopRated.
  ///
  /// In pt, this message translates to:
  /// **'Bem avaliados perto de você'**
  String get homeTopRated;

  /// No description provided for @homeSeeMore.
  ///
  /// In pt, this message translates to:
  /// **'Ver mais'**
  String get homeSeeMore;

  /// No description provided for @emptyCityTitle.
  ///
  /// In pt, this message translates to:
  /// **'Ainda não tem profissionais do Vitrio em {city}.'**
  String emptyCityTitle(String city);

  /// No description provided for @emptyCityTitleNearby.
  ///
  /// In pt, this message translates to:
  /// **'Ainda não tem profissionais do Vitrio perto de você.'**
  String get emptyCityTitleNearby;

  /// No description provided for @emptyCityText.
  ///
  /// In pt, this message translates to:
  /// **'Conhece alguém que faz um bom trabalho? Mande o Vitrio para essa pessoa.'**
  String get emptyCityText;

  /// No description provided for @emptyCityInvite.
  ///
  /// In pt, this message translates to:
  /// **'Indicar o Vitrio'**
  String get emptyCityInvite;

  /// No description provided for @emptyCityShareText.
  ///
  /// In pt, this message translates to:
  /// **'Conheça o Vitrio, o app para achar profissionais de confiança perto de você: {url}'**
  String emptyCityShareText(String url);

  /// No description provided for @favoritesTitle.
  ///
  /// In pt, this message translates to:
  /// **'Favoritos'**
  String get favoritesTitle;

  /// No description provided for @favoritesEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Você ainda não salvou nenhum profissional.'**
  String get favoritesEmpty;

  /// No description provided for @favoritesSearch.
  ///
  /// In pt, this message translates to:
  /// **'Buscar profissionais'**
  String get favoritesSearch;

  /// No description provided for @favoritesError.
  ///
  /// In pt, this message translates to:
  /// **'Não conseguimos carregar seus favoritos. Verifique a conexão e tente de novo.'**
  String get favoritesError;

  /// No description provided for @favoriteSave.
  ///
  /// In pt, this message translates to:
  /// **'Salvar nos favoritos'**
  String get favoriteSave;

  /// No description provided for @favoriteRemove.
  ///
  /// In pt, this message translates to:
  /// **'Remover dos favoritos'**
  String get favoriteRemove;

  /// No description provided for @favoriteFailed.
  ///
  /// In pt, this message translates to:
  /// **'Não conseguimos atualizar seus favoritos. Tente de novo.'**
  String get favoriteFailed;

  /// No description provided for @commonBack.
  ///
  /// In pt, this message translates to:
  /// **'Voltar'**
  String get commonBack;

  /// No description provided for @weekdaySunShort.
  ///
  /// In pt, this message translates to:
  /// **'dom'**
  String get weekdaySunShort;

  /// No description provided for @weekdayMonShort.
  ///
  /// In pt, this message translates to:
  /// **'seg'**
  String get weekdayMonShort;

  /// No description provided for @weekdayTueShort.
  ///
  /// In pt, this message translates to:
  /// **'ter'**
  String get weekdayTueShort;

  /// No description provided for @weekdayWedShort.
  ///
  /// In pt, this message translates to:
  /// **'qua'**
  String get weekdayWedShort;

  /// No description provided for @weekdayThuShort.
  ///
  /// In pt, this message translates to:
  /// **'qui'**
  String get weekdayThuShort;

  /// No description provided for @weekdayFriShort.
  ///
  /// In pt, this message translates to:
  /// **'sex'**
  String get weekdayFriShort;

  /// No description provided for @weekdaySatShort.
  ///
  /// In pt, this message translates to:
  /// **'sáb'**
  String get weekdaySatShort;

  /// No description provided for @orderToday.
  ///
  /// In pt, this message translates to:
  /// **'hoje'**
  String get orderToday;

  /// No description provided for @orderTomorrow.
  ///
  /// In pt, this message translates to:
  /// **'amanhã'**
  String get orderTomorrow;

  /// No description provided for @orderYesterday.
  ///
  /// In pt, this message translates to:
  /// **'ontem'**
  String get orderYesterday;

  /// No description provided for @orderDayAndTime.
  ///
  /// In pt, this message translates to:
  /// **'{day}, {time}'**
  String orderDayAndTime(String day, String time);

  /// No description provided for @orderPeriodMorning.
  ///
  /// In pt, this message translates to:
  /// **'Manhã'**
  String get orderPeriodMorning;

  /// No description provided for @orderPeriodAfternoon.
  ///
  /// In pt, this message translates to:
  /// **'Tarde'**
  String get orderPeriodAfternoon;

  /// No description provided for @orderPeriodEvening.
  ///
  /// In pt, this message translates to:
  /// **'Noite'**
  String get orderPeriodEvening;

  /// No description provided for @orderPeriodMorningHours.
  ///
  /// In pt, this message translates to:
  /// **'7h–12h'**
  String get orderPeriodMorningHours;

  /// No description provided for @orderPeriodAfternoonHours.
  ///
  /// In pt, this message translates to:
  /// **'12h–18h'**
  String get orderPeriodAfternoonHours;

  /// No description provided for @orderPeriodEveningHours.
  ///
  /// In pt, this message translates to:
  /// **'18h–22h'**
  String get orderPeriodEveningHours;

  /// No description provided for @orderPeriodMorningPhrase.
  ///
  /// In pt, this message translates to:
  /// **'{day} de manhã'**
  String orderPeriodMorningPhrase(String day);

  /// No description provided for @orderPeriodAfternoonPhrase.
  ///
  /// In pt, this message translates to:
  /// **'{day} à tarde'**
  String orderPeriodAfternoonPhrase(String day);

  /// No description provided for @orderPeriodEveningPhrase.
  ///
  /// In pt, this message translates to:
  /// **'{day} à noite'**
  String orderPeriodEveningPhrase(String day);

  /// No description provided for @orderStatusSent.
  ///
  /// In pt, this message translates to:
  /// **'Aguardando orçamento'**
  String get orderStatusSent;

  /// No description provided for @orderStatusQuoted.
  ///
  /// In pt, this message translates to:
  /// **'Orçamento recebido'**
  String get orderStatusQuoted;

  /// No description provided for @orderStatusScheduled.
  ///
  /// In pt, this message translates to:
  /// **'Agendado'**
  String get orderStatusScheduled;

  /// No description provided for @orderStatusDeclined.
  ///
  /// In pt, this message translates to:
  /// **'Recusado'**
  String get orderStatusDeclined;

  /// No description provided for @orderStatusExpired.
  ///
  /// In pt, this message translates to:
  /// **'Sem resposta'**
  String get orderStatusExpired;

  /// No description provided for @orderStatusQuoteDeclined.
  ///
  /// In pt, this message translates to:
  /// **'Orçamento recusado'**
  String get orderStatusQuoteDeclined;

  /// No description provided for @orderStatusQuoteExpired.
  ///
  /// In pt, this message translates to:
  /// **'Orçamento vencido'**
  String get orderStatusQuoteExpired;

  /// No description provided for @orderStatusCancelled.
  ///
  /// In pt, this message translates to:
  /// **'Cancelado'**
  String get orderStatusCancelled;

  /// No description provided for @orderStatusCompleted.
  ///
  /// In pt, this message translates to:
  /// **'Concluído'**
  String get orderStatusCompleted;

  /// No description provided for @orderStatusPaymentConfirmed.
  ///
  /// In pt, this message translates to:
  /// **'Pagamento confirmado'**
  String get orderStatusPaymentConfirmed;

  /// No description provided for @orderStatusPaymentDisputed.
  ///
  /// In pt, this message translates to:
  /// **'Pagamento contestado'**
  String get orderStatusPaymentDisputed;

  /// No description provided for @orderStatusPaymentVoided.
  ///
  /// In pt, this message translates to:
  /// **'Pagamento anulado'**
  String get orderStatusPaymentVoided;

  /// No description provided for @orderDeclineNoAvailability.
  ///
  /// In pt, this message translates to:
  /// **'Sem horário disponível'**
  String get orderDeclineNoAvailability;

  /// No description provided for @orderDeclineOutsideArea.
  ///
  /// In pt, this message translates to:
  /// **'Fora da minha área'**
  String get orderDeclineOutsideArea;

  /// No description provided for @orderDeclineNotMyService.
  ///
  /// In pt, this message translates to:
  /// **'Não faço esse serviço'**
  String get orderDeclineNotMyService;

  /// No description provided for @orderReasonOther.
  ///
  /// In pt, this message translates to:
  /// **'Outro'**
  String get orderReasonOther;

  /// No description provided for @orderCancelResolvedAnotherWay.
  ///
  /// In pt, this message translates to:
  /// **'Resolvi de outro jeito'**
  String get orderCancelResolvedAnotherWay;

  /// No description provided for @orderCancelTimeNoLongerWorks.
  ///
  /// In pt, this message translates to:
  /// **'O horário não serve mais'**
  String get orderCancelTimeNoLongerWorks;

  /// No description provided for @orderCancelNewTimeDoesNotWork.
  ///
  /// In pt, this message translates to:
  /// **'O novo horário não serve'**
  String get orderCancelNewTimeDoesNotWork;

  /// No description provided for @orderCancelFoundAnotherProfessional.
  ///
  /// In pt, this message translates to:
  /// **'Achei outro profissional'**
  String get orderCancelFoundAnotherProfessional;

  /// No description provided for @orderCancelUnforeseen.
  ///
  /// In pt, this message translates to:
  /// **'Imprevisto'**
  String get orderCancelUnforeseen;

  /// No description provided for @orderCancelCannotMakeIt.
  ///
  /// In pt, this message translates to:
  /// **'Não consigo chegar no horário'**
  String get orderCancelCannotMakeIt;

  /// No description provided for @orderCancelCustomerAskedToCancel.
  ///
  /// In pt, this message translates to:
  /// **'O cliente pediu para cancelar'**
  String get orderCancelCustomerAskedToCancel;

  /// No description provided for @orderCancelAccountDeleted.
  ///
  /// In pt, this message translates to:
  /// **'Conta excluída'**
  String get orderCancelAccountDeleted;

  /// No description provided for @orderCancelAccountSuspended.
  ///
  /// In pt, this message translates to:
  /// **'Conta suspensa'**
  String get orderCancelAccountSuspended;

  /// No description provided for @requestQuoteTitle.
  ///
  /// In pt, this message translates to:
  /// **'Pedir orçamento'**
  String get requestQuoteTitle;

  /// No description provided for @requestQuoteService.
  ///
  /// In pt, this message translates to:
  /// **'Qual serviço?'**
  String get requestQuoteService;

  /// No description provided for @requestQuoteChooseService.
  ///
  /// In pt, this message translates to:
  /// **'Escolha o serviço.'**
  String get requestQuoteChooseService;

  /// No description provided for @requestQuoteSummary.
  ///
  /// In pt, this message translates to:
  /// **'Resuma o que precisa'**
  String get requestQuoteSummary;

  /// No description provided for @requestQuoteSummaryPlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'Ex.: Chuveiro desarmando o disjuntor'**
  String get requestQuoteSummaryPlaceholder;

  /// No description provided for @requestQuoteSummaryInvalid.
  ///
  /// In pt, this message translates to:
  /// **'Resuma em 5 a 80 caracteres.'**
  String get requestQuoteSummaryInvalid;

  /// No description provided for @requestQuoteDescription.
  ///
  /// In pt, this message translates to:
  /// **'Descreva o que precisa'**
  String get requestQuoteDescription;

  /// No description provided for @requestQuoteDescriptionPlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'Ex.: a tomada da cozinha parou de funcionar depois de uma queda de energia.'**
  String get requestQuoteDescriptionPlaceholder;

  /// No description provided for @requestQuoteDescriptionHint.
  ///
  /// In pt, this message translates to:
  /// **'Quanto mais detalhes, mais preciso o orçamento.'**
  String get requestQuoteDescriptionHint;

  /// No description provided for @requestQuoteDescriptionInvalid.
  ///
  /// In pt, this message translates to:
  /// **'Descreva em 20 a 500 caracteres.'**
  String get requestQuoteDescriptionInvalid;

  /// No description provided for @requestQuotePhotos.
  ///
  /// In pt, this message translates to:
  /// **'Fotos'**
  String get requestQuotePhotos;

  /// No description provided for @requestQuoteOptional.
  ///
  /// In pt, this message translates to:
  /// **'(opcional)'**
  String get requestQuoteOptional;

  /// No description provided for @requestQuoteAddPhoto.
  ///
  /// In pt, this message translates to:
  /// **'Adicionar'**
  String get requestQuoteAddPhoto;

  /// No description provided for @requestQuoteRemovePhoto.
  ///
  /// In pt, this message translates to:
  /// **'Remover foto'**
  String get requestQuoteRemovePhoto;

  /// No description provided for @requestQuotePhotoFailed.
  ///
  /// In pt, this message translates to:
  /// **'Não conseguimos enviar a foto. Tente de novo.'**
  String get requestQuotePhotoFailed;

  /// No description provided for @requestQuoteWhen.
  ///
  /// In pt, this message translates to:
  /// **'Quando você precisa?'**
  String get requestQuoteWhen;

  /// No description provided for @requestQuoteDay.
  ///
  /// In pt, this message translates to:
  /// **'Dia'**
  String get requestQuoteDay;

  /// No description provided for @requestQuoteToday.
  ///
  /// In pt, this message translates to:
  /// **'Hoje'**
  String get requestQuoteToday;

  /// No description provided for @requestQuoteTomorrow.
  ///
  /// In pt, this message translates to:
  /// **'Amanhã'**
  String get requestQuoteTomorrow;

  /// No description provided for @requestQuotePickDate.
  ///
  /// In pt, this message translates to:
  /// **'Escolher data'**
  String get requestQuotePickDate;

  /// No description provided for @requestQuotePeriod.
  ///
  /// In pt, this message translates to:
  /// **'Período'**
  String get requestQuotePeriod;

  /// No description provided for @requestQuoteChoosePeriod.
  ///
  /// In pt, this message translates to:
  /// **'Escolha o período.'**
  String get requestQuoteChoosePeriod;

  /// No description provided for @requestQuotePeriodOver.
  ///
  /// In pt, this message translates to:
  /// **'Esse período de hoje já acabou.'**
  String get requestQuotePeriodOver;

  /// No description provided for @requestQuoteWhere.
  ///
  /// In pt, this message translates to:
  /// **'Onde?'**
  String get requestQuoteWhere;

  /// No description provided for @requestQuoteChangeAddress.
  ///
  /// In pt, this message translates to:
  /// **'Alterar'**
  String get requestQuoteChangeAddress;

  /// No description provided for @requestQuoteAddAddress.
  ///
  /// In pt, this message translates to:
  /// **'Adicionar'**
  String get requestQuoteAddAddress;

  /// No description provided for @requestQuoteNoAddress.
  ///
  /// In pt, this message translates to:
  /// **'Nenhum endereço cadastrado.'**
  String get requestQuoteNoAddress;

  /// No description provided for @requestQuoteAddressPrivacy.
  ///
  /// In pt, this message translates to:
  /// **'O endereço completo só é enviado ao profissional depois que você aceitar o orçamento.'**
  String get requestQuoteAddressPrivacy;

  /// No description provided for @requestQuotePhone.
  ///
  /// In pt, this message translates to:
  /// **'Seu celular'**
  String get requestQuotePhone;

  /// No description provided for @requestQuotePhoneHint.
  ///
  /// In pt, this message translates to:
  /// **'O profissional usa para falar com você depois que você aceitar o orçamento.'**
  String get requestQuotePhoneHint;

  /// No description provided for @requestQuotePhoneInvalid.
  ///
  /// In pt, this message translates to:
  /// **'Informe o celular com DDD.'**
  String get requestQuotePhoneInvalid;

  /// No description provided for @requestQuoteSend.
  ///
  /// In pt, this message translates to:
  /// **'Enviar pedido'**
  String get requestQuoteSend;

  /// No description provided for @requestQuoteFree.
  ///
  /// In pt, this message translates to:
  /// **'Pedir orçamento é grátis e sem compromisso.'**
  String get requestQuoteFree;

  /// No description provided for @addressSheetTitle.
  ///
  /// In pt, this message translates to:
  /// **'Onde vai ser o serviço?'**
  String get addressSheetTitle;

  /// No description provided for @addressNew.
  ///
  /// In pt, this message translates to:
  /// **'Novo endereço'**
  String get addressNew;

  /// No description provided for @addressCep.
  ///
  /// In pt, this message translates to:
  /// **'CEP'**
  String get addressCep;

  /// No description provided for @addressNumber.
  ///
  /// In pt, this message translates to:
  /// **'Número'**
  String get addressNumber;

  /// No description provided for @addressComplement.
  ///
  /// In pt, this message translates to:
  /// **'Complemento (opcional)'**
  String get addressComplement;

  /// No description provided for @addressLabel.
  ///
  /// In pt, this message translates to:
  /// **'Nome do endereço'**
  String get addressLabel;

  /// No description provided for @addressLabelPlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'Ex.: Casa, Trabalho'**
  String get addressLabelPlaceholder;

  /// No description provided for @addressLabelDefault.
  ///
  /// In pt, this message translates to:
  /// **'Casa'**
  String get addressLabelDefault;

  /// No description provided for @addressSave.
  ///
  /// In pt, this message translates to:
  /// **'Salvar endereço'**
  String get addressSave;

  /// No description provided for @addressSummary.
  ///
  /// In pt, this message translates to:
  /// **'{label} · {neighborhood}'**
  String addressSummary(String label, String neighborhood);

  /// No description provided for @addressCity.
  ///
  /// In pt, this message translates to:
  /// **'{city}, {state}'**
  String addressCity(String city, String state);

  /// No description provided for @orderScreenTitle.
  ///
  /// In pt, this message translates to:
  /// **'Seu pedido'**
  String get orderScreenTitle;

  /// No description provided for @orderSentLine.
  ///
  /// In pt, this message translates to:
  /// **'Enviado {when} · {neighborhood} · {preference}'**
  String orderSentLine(String when, String neighborhood, String preference);

  /// No description provided for @orderChatWith.
  ///
  /// In pt, this message translates to:
  /// **'Conversar com {name} pelo WhatsApp'**
  String orderChatWith(String name);

  /// No description provided for @orderProgress.
  ///
  /// In pt, this message translates to:
  /// **'Andamento'**
  String get orderProgress;

  /// No description provided for @orderStepSent.
  ///
  /// In pt, this message translates to:
  /// **'Pedido enviado'**
  String get orderStepSent;

  /// No description provided for @orderStepQuoted.
  ///
  /// In pt, this message translates to:
  /// **'Orçamento recebido'**
  String get orderStepQuoted;

  /// No description provided for @orderStepScheduled.
  ///
  /// In pt, this message translates to:
  /// **'Serviço agendado'**
  String get orderStepScheduled;

  /// No description provided for @orderStepCompleted.
  ///
  /// In pt, this message translates to:
  /// **'Serviço concluído'**
  String get orderStepCompleted;

  /// No description provided for @orderStepWaitingQuote.
  ///
  /// In pt, this message translates to:
  /// **'Responde até {time}'**
  String orderStepWaitingQuote(String time);

  /// No description provided for @orderStepWaitingYou.
  ///
  /// In pt, this message translates to:
  /// **'Aguardando sua resposta'**
  String get orderStepWaitingYou;

  /// No description provided for @orderStepConfirmWhenDone.
  ///
  /// In pt, this message translates to:
  /// **'Confirme quando terminar'**
  String get orderStepConfirmWhenDone;

  /// No description provided for @orderStepDone.
  ///
  /// In pt, this message translates to:
  /// **'Concluída'**
  String get orderStepDone;

  /// No description provided for @orderStepCurrent.
  ///
  /// In pt, this message translates to:
  /// **'Etapa atual'**
  String get orderStepCurrent;

  /// No description provided for @orderStepPending.
  ///
  /// In pt, this message translates to:
  /// **'Pendente'**
  String get orderStepPending;

  /// No description provided for @orderQuoteFrom.
  ///
  /// In pt, this message translates to:
  /// **'Orçamento de {name}'**
  String orderQuoteFrom(String name);

  /// No description provided for @orderQuoteValidUntil.
  ///
  /// In pt, this message translates to:
  /// **'Válido até {when}'**
  String orderQuoteValidUntil(String when);

  /// No description provided for @orderQuoteVisit.
  ///
  /// In pt, this message translates to:
  /// **'Visita {when}'**
  String orderQuoteVisit(String when);

  /// No description provided for @orderQuoteTechnicalVisit.
  ///
  /// In pt, this message translates to:
  /// **'Valor da visita técnica'**
  String get orderQuoteTechnicalVisit;

  /// No description provided for @orderQuoteMaterialIncluded.
  ///
  /// In pt, this message translates to:
  /// **'Material incluso'**
  String get orderQuoteMaterialIncluded;

  /// No description provided for @orderQuoteMaterialIncludedWhat.
  ///
  /// In pt, this message translates to:
  /// **'Material incluso ({what})'**
  String orderQuoteMaterialIncludedWhat(String what);

  /// No description provided for @orderQuoteMaterialNotIncluded.
  ///
  /// In pt, this message translates to:
  /// **'Material por sua conta'**
  String get orderQuoteMaterialNotIncluded;

  /// No description provided for @orderQuoteAccept.
  ///
  /// In pt, this message translates to:
  /// **'Aceitar orçamento'**
  String get orderQuoteAccept;

  /// No description provided for @orderQuoteDecline.
  ///
  /// In pt, this message translates to:
  /// **'Recusar'**
  String get orderQuoteDecline;

  /// No description provided for @orderQuoteAddressNote.
  ///
  /// In pt, this message translates to:
  /// **'Ao aceitar, {name} recebe seu endereço completo.'**
  String orderQuoteAddressNote(String name);

  /// No description provided for @orderScheduledTitle.
  ///
  /// In pt, this message translates to:
  /// **'Serviço agendado'**
  String get orderScheduledTitle;

  /// No description provided for @orderScheduledWhen.
  ///
  /// In pt, this message translates to:
  /// **'{day}\n{range}'**
  String orderScheduledWhen(String day, String range);

  /// No description provided for @orderScheduledBody.
  ///
  /// In pt, this message translates to:
  /// **'{name} já recebeu seu endereço completo. Valor combinado: {amount}.'**
  String orderScheduledBody(String name, String amount);

  /// No description provided for @orderScheduledCancel.
  ///
  /// In pt, this message translates to:
  /// **'Cancelar'**
  String get orderScheduledCancel;

  /// No description provided for @orderComplete.
  ///
  /// In pt, this message translates to:
  /// **'Serviço concluído'**
  String get orderComplete;

  /// No description provided for @orderCompleteFrom.
  ///
  /// In pt, this message translates to:
  /// **'Você poderá marcar como concluído a partir das {time}.'**
  String orderCompleteFrom(String time);

  /// No description provided for @orderRescheduled.
  ///
  /// In pt, this message translates to:
  /// **'{name} mudou o horário para {when}.'**
  String orderRescheduled(String name, String when);

  /// No description provided for @orderPaymentNote.
  ///
  /// In pt, this message translates to:
  /// **'O pagamento é combinado direto com o profissional. O Vitrio não cobra taxa de você.'**
  String get orderPaymentNote;

  /// No description provided for @orderDeclinedTitle.
  ///
  /// In pt, this message translates to:
  /// **'{name} não pode atender agora'**
  String orderDeclinedTitle(String name);

  /// No description provided for @orderExpiredTitle.
  ///
  /// In pt, this message translates to:
  /// **'{name} não respondeu a tempo'**
  String orderExpiredTitle(String name);

  /// No description provided for @orderDeclinedReason.
  ///
  /// In pt, this message translates to:
  /// **'Motivo: {reason}.'**
  String orderDeclinedReason(String reason);

  /// No description provided for @orderKeptForOthers.
  ///
  /// In pt, this message translates to:
  /// **'Seu pedido continua guardado. Peça para outro profissional com um toque.'**
  String get orderKeptForOthers;

  /// No description provided for @orderSuggestionsTitle.
  ///
  /// In pt, this message translates to:
  /// **'Outros {professionals} perto de você'**
  String orderSuggestionsTitle(String professionals);

  /// No description provided for @orderSuggestionsFallback.
  ///
  /// In pt, this message translates to:
  /// **'profissionais'**
  String get orderSuggestionsFallback;

  /// No description provided for @orderSuggestionsMore.
  ///
  /// In pt, this message translates to:
  /// **'Ver mais {professionals}'**
  String orderSuggestionsMore(String professionals);

  /// No description provided for @orderRequestThis.
  ///
  /// In pt, this message translates to:
  /// **'Pedir para este'**
  String get orderRequestThis;

  /// No description provided for @orderCancelledTitle.
  ///
  /// In pt, this message translates to:
  /// **'Pedido cancelado'**
  String get orderCancelledTitle;

  /// No description provided for @orderCancelledByYou.
  ///
  /// In pt, this message translates to:
  /// **'Você cancelou. Motivo: {reason}.'**
  String orderCancelledByYou(String reason);

  /// No description provided for @orderCancelledByProvider.
  ///
  /// In pt, this message translates to:
  /// **'{name} cancelou. Motivo: {reason}.'**
  String orderCancelledByProvider(String name, String reason);

  /// No description provided for @orderCancelledBySystem.
  ///
  /// In pt, this message translates to:
  /// **'Cancelado pelo Vitrio. Motivo: {reason}.'**
  String orderCancelledBySystem(String reason);

  /// No description provided for @orderCompletedTitle.
  ///
  /// In pt, this message translates to:
  /// **'Serviço concluído'**
  String get orderCompletedTitle;

  /// No description provided for @orderCompletedOn.
  ///
  /// In pt, this message translates to:
  /// **'Concluído em {when}.'**
  String orderCompletedOn(String when);

  /// No description provided for @orderQuoteDeclinedTitle.
  ///
  /// In pt, this message translates to:
  /// **'Você recusou o orçamento'**
  String get orderQuoteDeclinedTitle;

  /// No description provided for @orderQuoteExpiredTitle.
  ///
  /// In pt, this message translates to:
  /// **'O orçamento venceu sem resposta'**
  String get orderQuoteExpiredTitle;

  /// No description provided for @orderPhotosRemoved.
  ///
  /// In pt, this message translates to:
  /// **'Fotos removidas'**
  String get orderPhotosRemoved;

  /// No description provided for @orderLoadError.
  ///
  /// In pt, this message translates to:
  /// **'Não conseguimos carregar o pedido. Verifique a conexão e tente de novo.'**
  String get orderLoadError;

  /// No description provided for @orderQuoteChanged.
  ///
  /// In pt, this message translates to:
  /// **'{name} mudou o orçamento. Confira o novo valor antes de aceitar.'**
  String orderQuoteChanged(String name);

  /// No description provided for @cancelOrderTitle.
  ///
  /// In pt, this message translates to:
  /// **'Cancelar o pedido?'**
  String get cancelOrderTitle;

  /// No description provided for @cancelOrderSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'{name} vê o motivo que você escolher, nunca o que você escrever.'**
  String cancelOrderSubtitle(String name);

  /// No description provided for @cancelOrderReasons.
  ///
  /// In pt, this message translates to:
  /// **'Motivo'**
  String get cancelOrderReasons;

  /// No description provided for @cancelOrderDetailsOptional.
  ///
  /// In pt, this message translates to:
  /// **'Quer contar mais? (opcional)'**
  String get cancelOrderDetailsOptional;

  /// No description provided for @cancelOrderDetailsRequired.
  ///
  /// In pt, this message translates to:
  /// **'Conte o motivo'**
  String get cancelOrderDetailsRequired;

  /// No description provided for @cancelOrderDetailsPlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'Conte o que aconteceu.'**
  String get cancelOrderDetailsPlaceholder;

  /// No description provided for @cancelOrderChooseReason.
  ///
  /// In pt, this message translates to:
  /// **'Escolha um motivo.'**
  String get cancelOrderChooseReason;

  /// No description provided for @cancelOrderConfirm.
  ///
  /// In pt, this message translates to:
  /// **'Cancelar pedido'**
  String get cancelOrderConfirm;

  /// No description provided for @ordersOpenTab.
  ///
  /// In pt, this message translates to:
  /// **'Em andamento'**
  String get ordersOpenTab;

  /// No description provided for @ordersClosedTab.
  ///
  /// In pt, this message translates to:
  /// **'Finalizados'**
  String get ordersClosedTab;

  /// No description provided for @ordersClosedEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Nenhum pedido finalizado ainda.'**
  String get ordersClosedEmpty;

  /// No description provided for @ordersSignedOut.
  ///
  /// In pt, this message translates to:
  /// **'Entre para pedir orçamentos e acompanhar seus pedidos.'**
  String get ordersSignedOut;

  /// No description provided for @ordersLoadError.
  ///
  /// In pt, this message translates to:
  /// **'Não conseguimos carregar seus pedidos. Verifique a conexão e tente de novo.'**
  String get ordersLoadError;

  /// No description provided for @ordersSentMeta.
  ///
  /// In pt, this message translates to:
  /// **'{provider} · enviado {when}'**
  String ordersSentMeta(String provider, String when);

  /// No description provided for @ordersScheduledMeta.
  ///
  /// In pt, this message translates to:
  /// **'{provider} · {when}'**
  String ordersScheduledMeta(String provider, String when);

  /// No description provided for @homeLastOrder.
  ///
  /// In pt, this message translates to:
  /// **'Seu último pedido'**
  String get homeLastOrder;

  /// No description provided for @responseTime15min.
  ///
  /// In pt, this message translates to:
  /// **'~15 min'**
  String get responseTime15min;

  /// No description provided for @responseTime30min.
  ///
  /// In pt, this message translates to:
  /// **'~30 min'**
  String get responseTime30min;

  /// No description provided for @responseTime1h.
  ///
  /// In pt, this message translates to:
  /// **'~1 h'**
  String get responseTime1h;

  /// No description provided for @responseTime3h.
  ///
  /// In pt, this message translates to:
  /// **'~3 h'**
  String get responseTime3h;

  /// No description provided for @responseTime1d.
  ///
  /// In pt, this message translates to:
  /// **'~1 dia'**
  String get responseTime1d;

  /// No description provided for @requestQuoteAddressRequired.
  ///
  /// In pt, this message translates to:
  /// **'Adicione o endereço onde o serviço vai ser feito.'**
  String get requestQuoteAddressRequired;

  /// No description provided for @providerOrdersNewTab.
  ///
  /// In pt, this message translates to:
  /// **'Novos'**
  String get providerOrdersNewTab;

  /// No description provided for @providerOrdersNewTabCount.
  ///
  /// In pt, this message translates to:
  /// **'Novos · {count}'**
  String providerOrdersNewTabCount(int count);

  /// No description provided for @providerOrdersOngoingEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Nenhum pedido em andamento.'**
  String get providerOrdersOngoingEmpty;

  /// No description provided for @providerOrdersClosedEmpty.
  ///
  /// In pt, this message translates to:
  /// **'Nenhum pedido finalizado ainda.'**
  String get providerOrdersClosedEmpty;

  /// No description provided for @providerNewOrderBadge.
  ///
  /// In pt, this message translates to:
  /// **'Novo · {service}'**
  String providerNewOrderBadge(String service);

  /// No description provided for @orderAgoMinutes.
  ///
  /// In pt, this message translates to:
  /// **'há {count} min'**
  String orderAgoMinutes(int count);

  /// No description provided for @orderAgoHours.
  ///
  /// In pt, this message translates to:
  /// **'há {count} h'**
  String orderAgoHours(int count);

  /// No description provided for @orderAgoDays.
  ///
  /// In pt, this message translates to:
  /// **'{count, plural, =1{há 1 dia} other{há {count} dias}}'**
  String orderAgoDays(int count);

  /// No description provided for @orderPreferenceLine.
  ///
  /// In pt, this message translates to:
  /// **'{day} · {period} ({hours})'**
  String orderPreferenceLine(String day, String period, String hours);

  /// No description provided for @orderRespondBy.
  ///
  /// In pt, this message translates to:
  /// **'Responda até {when}'**
  String orderRespondBy(String when);

  /// No description provided for @orderPhotosAttached.
  ///
  /// In pt, this message translates to:
  /// **'{count, plural, =1{1 foto anexada} other{{count} fotos anexadas}}'**
  String orderPhotosAttached(int count);

  /// No description provided for @providerSendQuote.
  ///
  /// In pt, this message translates to:
  /// **'Enviar orçamento'**
  String get providerSendQuote;

  /// No description provided for @providerSeeAllOrders.
  ///
  /// In pt, this message translates to:
  /// **'Ver todos'**
  String get providerSeeAllOrders;

  /// No description provided for @providerStatusQuoted.
  ///
  /// In pt, this message translates to:
  /// **'Aguardando · válido até {when}'**
  String providerStatusQuoted(String when);

  /// No description provided for @providerStatusScheduled.
  ///
  /// In pt, this message translates to:
  /// **'Agendado · {when}'**
  String providerStatusScheduled(String when);

  /// No description provided for @providerStatusDeclined.
  ///
  /// In pt, this message translates to:
  /// **'Recusado por você'**
  String get providerStatusDeclined;

  /// No description provided for @providerStatusExpired.
  ///
  /// In pt, this message translates to:
  /// **'Venceu sem resposta'**
  String get providerStatusExpired;

  /// No description provided for @providerStatusCancelledByCustomer.
  ///
  /// In pt, this message translates to:
  /// **'Cancelado pelo cliente'**
  String get providerStatusCancelledByCustomer;

  /// No description provided for @providerStatusCancelledByYou.
  ///
  /// In pt, this message translates to:
  /// **'Cancelado por você'**
  String get providerStatusCancelledByYou;

  /// No description provided for @providerStatusCancelledBySystem.
  ///
  /// In pt, this message translates to:
  /// **'Cancelado pelo Vitrio'**
  String get providerStatusCancelledBySystem;

  /// No description provided for @respondTitle.
  ///
  /// In pt, this message translates to:
  /// **'Responder pedido'**
  String get respondTitle;

  /// No description provided for @respondMemberSince.
  ///
  /// In pt, this message translates to:
  /// **'No Vitrio desde {month} · {count, plural, =1{1 pedido} other{{count} pedidos}}'**
  String respondMemberSince(String month, int count);

  /// No description provided for @respondWhen.
  ///
  /// In pt, this message translates to:
  /// **'Quando'**
  String get respondWhen;

  /// No description provided for @respondWhere.
  ///
  /// In pt, this message translates to:
  /// **'Onde'**
  String get respondWhere;

  /// No description provided for @respondAddressLocked.
  ///
  /// In pt, this message translates to:
  /// **'Endereço completo liberado quando {name} aceitar.'**
  String respondAddressLocked(String name);

  /// No description provided for @respondYourQuote.
  ///
  /// In pt, this message translates to:
  /// **'Seu orçamento'**
  String get respondYourQuote;

  /// No description provided for @respondQuoteType.
  ///
  /// In pt, this message translates to:
  /// **'Tipo de orçamento'**
  String get respondQuoteType;

  /// No description provided for @quoteTypeFixedPrice.
  ///
  /// In pt, this message translates to:
  /// **'Valor fechado'**
  String get quoteTypeFixedPrice;

  /// No description provided for @quoteTypeTechnicalVisit.
  ///
  /// In pt, this message translates to:
  /// **'Visita técnica'**
  String get quoteTypeTechnicalVisit;

  /// No description provided for @respondPriceFixed.
  ///
  /// In pt, this message translates to:
  /// **'Valor do serviço'**
  String get respondPriceFixed;

  /// No description provided for @respondPriceVisit.
  ///
  /// In pt, this message translates to:
  /// **'Valor da visita técnica'**
  String get respondPriceVisit;

  /// No description provided for @respondPriceInvalid.
  ///
  /// In pt, this message translates to:
  /// **'Informe um valor entre R\$ 1,00 e R\$ 100.000,00.'**
  String get respondPriceInvalid;

  /// No description provided for @respondDay.
  ///
  /// In pt, this message translates to:
  /// **'Dia da visita'**
  String get respondDay;

  /// No description provided for @respondStart.
  ///
  /// In pt, this message translates to:
  /// **'Início'**
  String get respondStart;

  /// No description provided for @respondDuration.
  ///
  /// In pt, this message translates to:
  /// **'Duração'**
  String get respondDuration;

  /// No description provided for @respondRange.
  ///
  /// In pt, this message translates to:
  /// **'Das {start} às {end}'**
  String respondRange(String start, String end);

  /// No description provided for @respondOverlap.
  ///
  /// In pt, this message translates to:
  /// **'Você já tem um serviço das {start} às {end} nesse dia.'**
  String respondOverlap(String start, String end);

  /// No description provided for @respondMaterial.
  ///
  /// In pt, this message translates to:
  /// **'Material incluso'**
  String get respondMaterial;

  /// No description provided for @respondMaterialYours.
  ///
  /// In pt, this message translates to:
  /// **'Você leva o material'**
  String get respondMaterialYours;

  /// No description provided for @respondMaterialCustomer.
  ///
  /// In pt, this message translates to:
  /// **'{name} compra o material'**
  String respondMaterialCustomer(String name);

  /// No description provided for @respondMaterialWhat.
  ///
  /// In pt, this message translates to:
  /// **'Qual material'**
  String get respondMaterialWhat;

  /// No description provided for @respondMaterialPlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'Ex.: disjuntor 40 A e conectores'**
  String get respondMaterialPlaceholder;

  /// No description provided for @respondMessage.
  ///
  /// In pt, this message translates to:
  /// **'Mensagem para {name}'**
  String respondMessage(String name);

  /// No description provided for @respondMessagePlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'Explique o que está incluso e o que você vai fazer.'**
  String get respondMessagePlaceholder;

  /// No description provided for @respondNote.
  ///
  /// In pt, this message translates to:
  /// **'{name} recebe uma notificação e aceita pelo app.'**
  String respondNote(String name);

  /// No description provided for @respondReplaceNote.
  ///
  /// In pt, this message translates to:
  /// **'Você já enviou um orçamento de {amount}. O novo substitui o anterior e a validade recomeça.'**
  String respondReplaceNote(String amount);

  /// No description provided for @durationMinutes.
  ///
  /// In pt, this message translates to:
  /// **'{minutes} min'**
  String durationMinutes(int minutes);

  /// No description provided for @durationHours.
  ///
  /// In pt, this message translates to:
  /// **'{hours} h'**
  String durationHours(int hours);

  /// No description provided for @durationHoursMinutes.
  ///
  /// In pt, this message translates to:
  /// **'{hours} h {minutes}'**
  String durationHoursMinutes(int hours, int minutes);

  /// No description provided for @declineOrderTitle.
  ///
  /// In pt, this message translates to:
  /// **'Recusar o pedido?'**
  String get declineOrderTitle;

  /// No description provided for @declineOrderSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'{name} vê o motivo junto de “{provider} não pode atender agora” e recebe sugestões de outros profissionais.'**
  String declineOrderSubtitle(String name, String provider);

  /// No description provided for @declineOrderDetails.
  ///
  /// In pt, this message translates to:
  /// **'Explique em poucas palavras'**
  String get declineOrderDetails;

  /// No description provided for @declineOrderPlaceholder.
  ///
  /// In pt, this message translates to:
  /// **'{name} vai ler este texto.'**
  String declineOrderPlaceholder(String name);

  /// No description provided for @declineOrderHint.
  ///
  /// In pt, this message translates to:
  /// **'Sem telefone, e-mail ou link.'**
  String get declineOrderHint;

  /// No description provided for @declineOrderDetailsRequired.
  ///
  /// In pt, this message translates to:
  /// **'Conte o motivo em poucas palavras.'**
  String get declineOrderDetailsRequired;

  /// No description provided for @declineOrderConfirm.
  ///
  /// In pt, this message translates to:
  /// **'Recusar pedido'**
  String get declineOrderConfirm;

  /// No description provided for @providerOrderTitle.
  ///
  /// In pt, this message translates to:
  /// **'Pedido'**
  String get providerOrderTitle;

  /// No description provided for @providerOrderMeta.
  ///
  /// In pt, this message translates to:
  /// **'{service} · pedido enviado {when}'**
  String providerOrderMeta(String service, String when);

  /// No description provided for @providerAgreedValue.
  ///
  /// In pt, this message translates to:
  /// **'Valor combinado: {amount}.'**
  String providerAgreedValue(String amount);

  /// No description provided for @providerMaterialIncluded.
  ///
  /// In pt, this message translates to:
  /// **'Material incluso ({what}).'**
  String providerMaterialIncluded(String what);

  /// No description provided for @providerMaterialIncludedPlain.
  ///
  /// In pt, this message translates to:
  /// **'Material incluso.'**
  String get providerMaterialIncludedPlain;

  /// No description provided for @providerReschedule.
  ///
  /// In pt, this message translates to:
  /// **'Ajustar horário'**
  String get providerReschedule;

  /// No description provided for @providerCompleteFrom.
  ///
  /// In pt, this message translates to:
  /// **'Você pode marcar como concluído a partir das {time} de {day}.'**
  String providerCompleteFrom(String time, String day);

  /// No description provided for @providerCustomerSection.
  ///
  /// In pt, this message translates to:
  /// **'Cliente'**
  String get providerCustomerSection;

  /// No description provided for @providerCall.
  ///
  /// In pt, this message translates to:
  /// **'Ligar'**
  String get providerCall;

  /// No description provided for @providerAddressSection.
  ///
  /// In pt, this message translates to:
  /// **'Endereço'**
  String get providerAddressSection;

  /// No description provided for @providerOpenMap.
  ///
  /// In pt, this message translates to:
  /// **'Abrir no mapa'**
  String get providerOpenMap;

  /// No description provided for @providerContactVisibleUntil.
  ///
  /// In pt, this message translates to:
  /// **'Endereço e contato visíveis até {day}.'**
  String providerContactVisibleUntil(String day);

  /// No description provided for @providerContactHidden.
  ///
  /// In pt, this message translates to:
  /// **'O endereço e o contato não aparecem mais neste pedido.'**
  String get providerContactHidden;

  /// No description provided for @providerOrderSection.
  ///
  /// In pt, this message translates to:
  /// **'Pedido'**
  String get providerOrderSection;

  /// No description provided for @providerCustomerAsked.
  ///
  /// In pt, this message translates to:
  /// **'{name} pediu: {preference}'**
  String providerCustomerAsked(String name, String preference);

  /// No description provided for @providerQuoteSentTitle.
  ///
  /// In pt, this message translates to:
  /// **'Orçamento enviado'**
  String get providerQuoteSentTitle;

  /// No description provided for @providerQuoteWaiting.
  ///
  /// In pt, this message translates to:
  /// **'{name} pode aceitar até {when}.'**
  String providerQuoteWaiting(String name, String when);

  /// No description provided for @providerReplaceQuote.
  ///
  /// In pt, this message translates to:
  /// **'Substituir orçamento'**
  String get providerReplaceQuote;

  /// No description provided for @providerDeclinedTitle.
  ///
  /// In pt, this message translates to:
  /// **'Você recusou o pedido'**
  String get providerDeclinedTitle;

  /// No description provided for @providerExpiredTitle.
  ///
  /// In pt, this message translates to:
  /// **'O pedido venceu sem resposta'**
  String get providerExpiredTitle;

  /// No description provided for @providerQuoteDeclinedTitle.
  ///
  /// In pt, this message translates to:
  /// **'{name} recusou o orçamento'**
  String providerQuoteDeclinedTitle(String name);

  /// No description provided for @providerQuoteExpiredTitle.
  ///
  /// In pt, this message translates to:
  /// **'O orçamento venceu sem resposta'**
  String get providerQuoteExpiredTitle;

  /// No description provided for @providerCancelledByCustomer.
  ///
  /// In pt, this message translates to:
  /// **'{name} cancelou. Motivo: {reason}.'**
  String providerCancelledByCustomer(String name, String reason);

  /// No description provided for @rescheduleTitle.
  ///
  /// In pt, this message translates to:
  /// **'Ajustar horário'**
  String get rescheduleTitle;

  /// No description provided for @rescheduleCurrent.
  ///
  /// In pt, this message translates to:
  /// **'Horário atual'**
  String get rescheduleCurrent;

  /// No description provided for @rescheduleNew.
  ///
  /// In pt, this message translates to:
  /// **'Novo horário'**
  String get rescheduleNew;

  /// No description provided for @rescheduleDay.
  ///
  /// In pt, this message translates to:
  /// **'Dia'**
  String get rescheduleDay;

  /// No description provided for @rescheduleRemaining.
  ///
  /// In pt, this message translates to:
  /// **'{count, plural, =1{Você ainda pode ajustar este pedido mais 1 vez.} other{Você ainda pode ajustar este pedido mais {count} vezes.}}'**
  String rescheduleRemaining(int count);

  /// No description provided for @rescheduleNote.
  ///
  /// In pt, this message translates to:
  /// **'{name} recebe um aviso com o novo horário e pode cancelar se não servir.'**
  String rescheduleNote(String name);

  /// No description provided for @rescheduleSave.
  ///
  /// In pt, this message translates to:
  /// **'Salvar novo horário'**
  String get rescheduleSave;

  /// No description provided for @rescheduleNone.
  ///
  /// In pt, this message translates to:
  /// **'Você já ajustou o horário deste pedido 2 vezes.'**
  String get rescheduleNone;

  /// No description provided for @orderPhotoLabel.
  ///
  /// In pt, this message translates to:
  /// **'Foto {index} do pedido'**
  String orderPhotoLabel(int index);

  /// No description provided for @providerRescheduled.
  ///
  /// In pt, this message translates to:
  /// **'Novo horário salvo. {name} recebe um aviso.'**
  String providerRescheduled(String name);

  /// No description provided for @providerOrderCompleted.
  ///
  /// In pt, this message translates to:
  /// **'Serviço marcado como concluído.'**
  String get providerOrderCompleted;

  /// No description provided for @providerOrderCancelled.
  ///
  /// In pt, this message translates to:
  /// **'Pedido cancelado.'**
  String get providerOrderCancelled;

  /// No description provided for @agendaSubtitle.
  ///
  /// In pt, this message translates to:
  /// **'Seus serviços e os orçamentos que aguardam resposta.'**
  String get agendaSubtitle;

  /// No description provided for @agendaPreviousMonth.
  ///
  /// In pt, this message translates to:
  /// **'Mês anterior'**
  String get agendaPreviousMonth;

  /// No description provided for @agendaNextMonth.
  ///
  /// In pt, this message translates to:
  /// **'Próximo mês'**
  String get agendaNextMonth;

  /// No description provided for @agendaScheduled.
  ///
  /// In pt, this message translates to:
  /// **'Agendado'**
  String get agendaScheduled;

  /// No description provided for @agendaQuoted.
  ///
  /// In pt, this message translates to:
  /// **'Aguardando o cliente'**
  String get agendaQuoted;

  /// No description provided for @agendaDone.
  ///
  /// In pt, this message translates to:
  /// **'Feito'**
  String get agendaDone;

  /// No description provided for @agendaScheduledSection.
  ///
  /// In pt, this message translates to:
  /// **'Agendados'**
  String get agendaScheduledSection;

  /// No description provided for @agendaQuotedSection.
  ///
  /// In pt, this message translates to:
  /// **'Aguardando o cliente'**
  String get agendaQuotedSection;

  /// No description provided for @agendaDoneSection.
  ///
  /// In pt, this message translates to:
  /// **'Feitos'**
  String get agendaDoneSection;

  /// No description provided for @agendaDayTitle.
  ///
  /// In pt, this message translates to:
  /// **'{weekday}, {day} de {month}'**
  String agendaDayTitle(String weekday, int day, String month);

  /// No description provided for @agendaDayLabel.
  ///
  /// In pt, this message translates to:
  /// **'{weekday}, {day} de {month}{count, plural, =0{} =1{, 1 item} other{, {count} itens}}'**
  String agendaDayLabel(String weekday, int day, String month, int count);

  /// No description provided for @agendaUntil.
  ///
  /// In pt, this message translates to:
  /// **'até {time}'**
  String agendaUntil(String time);

  /// No description provided for @agendaEmptyDay.
  ///
  /// In pt, this message translates to:
  /// **'Nenhum serviço neste dia.'**
  String get agendaEmptyDay;

  /// No description provided for @agendaLoadError.
  ///
  /// In pt, this message translates to:
  /// **'Não conseguimos carregar a agenda. Verifique a conexão e tente de novo.'**
  String get agendaLoadError;

  /// No description provided for @addressCepHint.
  ///
  /// In pt, this message translates to:
  /// **'Digite o CEP para preencher o endereço.'**
  String get addressCepHint;

  /// No description provided for @addressCepSearching.
  ///
  /// In pt, this message translates to:
  /// **'Buscando o endereço…'**
  String get addressCepSearching;

  /// No description provided for @addressCepNotFound.
  ///
  /// In pt, this message translates to:
  /// **'Não encontramos este CEP. Confira os 8 números.'**
  String get addressCepNotFound;

  /// No description provided for @addressStreetLabel.
  ///
  /// In pt, this message translates to:
  /// **'Rua'**
  String get addressStreetLabel;

  /// No description provided for @addressNeighborhoodLabel.
  ///
  /// In pt, this message translates to:
  /// **'Bairro'**
  String get addressNeighborhoodLabel;

  /// No description provided for @addressCityLabel.
  ///
  /// In pt, this message translates to:
  /// **'Cidade'**
  String get addressCityLabel;

  /// No description provided for @addressStreetRequired.
  ///
  /// In pt, this message translates to:
  /// **'Informe a rua.'**
  String get addressStreetRequired;

  /// No description provided for @addressNeighborhoodRequired.
  ///
  /// In pt, this message translates to:
  /// **'Informe o bairro.'**
  String get addressNeighborhoodRequired;

  /// No description provided for @addressNumberRequired.
  ///
  /// In pt, this message translates to:
  /// **'Informe o número.'**
  String get addressNumberRequired;
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
