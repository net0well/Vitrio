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
      'Encontre, contrate e avalie eletricistas, diaristas, pedreiros e muito mais, com a nota de quem já contratou.';

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
      'Entre ou crie sua conta para pedir orçamentos e oferecer seus serviços.';

  @override
  String get authSignIn => 'Entrar';

  @override
  String get authCreateAccount => 'Criar conta';

  @override
  String get authEmail => 'E-mail';

  @override
  String get authPassword => 'Senha';

  @override
  String get authShowPassword => 'Mostrar senha';

  @override
  String get authHidePassword => 'Esconder senha';

  @override
  String get authForgotPassword => 'Esqueci a senha';

  @override
  String get authName => 'Nome';

  @override
  String get authPasswordRule =>
      'Use pelo menos 10 caracteres. Uma frase com espaços vale.';

  @override
  String get authAcceptTerms =>
      'Li e aceito os termos de uso e a política de privacidade.';

  @override
  String get authAcceptTermsRequired => 'Aceite os termos para continuar.';

  @override
  String get authVerifyTitle => 'Confirme seu e-mail';

  @override
  String authVerifyMessage(String email) {
    return 'Enviamos um código de 6 números para $email. Digite o código para terminar.';
  }

  @override
  String get authCode => 'Código';

  @override
  String get authConfirm => 'Confirmar';

  @override
  String get authResendCode => 'Reenviar código';

  @override
  String authResendIn(int seconds) {
    return 'Reenviar em $seconds s';
  }

  @override
  String get authCodeResent => 'Enviamos um novo código.';

  @override
  String get authChangeEmail => 'Trocar e-mail';

  @override
  String get authForgotTitle => 'Redefinir senha';

  @override
  String get authForgotMessage =>
      'Digite o e-mail da sua conta. Vamos enviar um código para você criar uma senha nova.';

  @override
  String get authSendCode => 'Enviar código';

  @override
  String get authCodeSentIfExists =>
      'Se houver uma conta com este e-mail, enviamos um código para ele.';

  @override
  String get authNewPassword => 'Senha nova';

  @override
  String get authSaveNewPassword => 'Salvar senha nova';

  @override
  String get errorConnection =>
      'Sem conexão. Confira a internet e tente de novo.';

  @override
  String get errorGeneric => 'Algo deu errado. Tente de novo.';

  @override
  String get commonRetry => 'Tentar de novo';

  @override
  String get commonCancel => 'Cancelar';

  @override
  String get commonSave => 'Salvar';

  @override
  String get accountEditProfile => 'Editar dados';

  @override
  String get accountChangePassword => 'Trocar senha';

  @override
  String get accountDevices => 'Aparelhos conectados';

  @override
  String get accountSignOut => 'Sair';

  @override
  String get accountDelete => 'Excluir conta';

  @override
  String get accountDeleteTitle => 'Excluir sua conta?';

  @override
  String get accountDeleteMessage =>
      'Seus dados pessoais serão apagados e você sai de todos os aparelhos. Isso não pode ser desfeito. Para confirmar, digite EXCLUIR.';

  @override
  String get accountDeleteWord => 'EXCLUIR';

  @override
  String get accountDeleteConfirm => 'Excluir';

  @override
  String get profilePhone => 'Celular';

  @override
  String get profilePhoneHint => '(16) 99123-4567';

  @override
  String get profileSaved => 'Dados salvos.';

  @override
  String get passwordCurrent => 'Senha atual';

  @override
  String get passwordChanged =>
      'Senha trocada. Os outros aparelhos foram desconectados.';

  @override
  String get devicesThisDevice => 'Este aparelho';

  @override
  String devicesLastSeen(String date) {
    return 'Último acesso em $date';
  }

  @override
  String get devicesDisconnect => 'Desconectar';

  @override
  String get devicesSignOutAll => 'Sair de todos os aparelhos';

  @override
  String get devicesOnlyThis => 'Só este aparelho está conectado.';

  @override
  String get locationPermissionTitle =>
      'Para mostrar quem atende perto de você';

  @override
  String get locationPermissionMessage =>
      'O Vitrio usa sua localização só enquanto o app está aberto e não guarda o histórico. Se preferir, escolha a cidade.';

  @override
  String get locationUseMine => 'Usar minha localização';

  @override
  String get locationChooseCity => 'Escolher cidade';

  @override
  String get locationSearchHint => 'Buscar cidade';

  @override
  String get locationRecent => 'Recentes';

  @override
  String get locationNoResults => 'Nenhuma cidade encontrada.';

  @override
  String locationShowingCity(String city) {
    return 'Mostrando profissionais de $city.';
  }

  @override
  String get locationDenied => 'Sem acesso à localização. Escolha a cidade.';

  @override
  String get locationNotFound =>
      'Não encontramos sua cidade. Escolha na lista.';

  @override
  String get locationPillHint => 'Trocar cidade';

  @override
  String get tabDashboard => 'Painel';

  @override
  String get tabMyProfile => 'Meu perfil';

  @override
  String get providerOfferServices => 'Quero oferecer meus serviços';

  @override
  String get providerEnterMode => 'Entrar como prestador';

  @override
  String get providerLeaveMode => 'Voltar para cliente';

  @override
  String providerStepOf(int step, int total) {
    return 'Passo $step de $total';
  }

  @override
  String get providerStepAbout => 'Sobre você';

  @override
  String get providerStepShowcase => 'Monte sua vitrine';

  @override
  String get providerStepPhotos => 'Fotos e preços';

  @override
  String get providerStepReview => 'Revisar e publicar';

  @override
  String get providerContinue => 'Continuar';

  @override
  String get providerPublish => 'Publicar';

  @override
  String get providerDisplayName => 'Nome que os clientes vão ver';

  @override
  String get providerHeadline => 'Profissão';

  @override
  String get providerHeadlineHint => 'Ex.: Eletricista';

  @override
  String get providerAbout => 'Sobre você e o seu trabalho';

  @override
  String get providerAboutHint =>
      'Conte há quanto tempo trabalha, o que faz e o que te diferencia.';

  @override
  String get providerYears => 'Anos de experiência';

  @override
  String get providerAvatarAdd => 'Adicionar foto de perfil';

  @override
  String get providerAvatarChange => 'Trocar foto de perfil';

  @override
  String get providerCategories => 'O que você faz?';

  @override
  String get providerCategoriesHint =>
      'Escolha até 3 categorias. A primeira é a principal.';

  @override
  String get providerCategoriesRequired => 'Escolha ao menos uma categoria.';

  @override
  String get providerPrimary => 'Principal';

  @override
  String get providerServiceArea => 'Onde você atende?';

  @override
  String get providerCep => 'CEP do endereço base';

  @override
  String get providerNumber => 'Número';

  @override
  String get providerComplement => 'Complemento';

  @override
  String get providerBasePrivate =>
      'Seu endereço não aparece para ninguém. Os clientes veem só a cidade, os bairros que você atende e a distância aproximada.';

  @override
  String providerRadius(int km) {
    return 'até $km km';
  }

  @override
  String get providerNeighborhoods => 'Bairros atendidos';

  @override
  String get providerNeighborhoodHint => 'Digite o bairro e toque em adicionar';

  @override
  String get providerNeighborhoodAdd => 'Adicionar bairro';

  @override
  String get providerApproximate =>
      'Não achamos o ponto exato do seu CEP. Usamos o centro da cidade, então as distâncias ficam aproximadas.';

  @override
  String get providerContact => 'Contato';

  @override
  String get providerContactPhone => 'WhatsApp para contato';

  @override
  String get providerWhatsApp => 'Mostrar o botão de WhatsApp no perfil';

  @override
  String get providerWhatsAppHint =>
      'Sem ele, o contato é só pelo pedido no app.';

  @override
  String get providerMei => 'Tem MEI? Ganhe o selo verificado';

  @override
  String get providerMeiHint => 'Consultamos seu CNPJ na Receita Federal.';

  @override
  String get providerCnpj => 'CNPJ';

  @override
  String get providerVerify => 'Verificar';

  @override
  String get providerMeiVerified =>
      'CNPJ ativo e optante pelo MEI. O selo já aparece no seu perfil.';

  @override
  String get providerMeiPending =>
      'Consulta pendente. Vamos tentar de novo nas próximas horas.';

  @override
  String get providerMeiLost => 'O selo MEI saiu na última revalidação.';

  @override
  String get providerPhotos => 'Fotos dos seus trabalhos';

  @override
  String get providerPhotosHint =>
      'Até 30 fotos. Elas aparecem no perfil depois de processadas.';

  @override
  String get providerPhotoAdd => 'Adicionar foto';

  @override
  String get providerPhotoProcessing => 'Processando';

  @override
  String get providerPhotoRejected => 'Recusada';

  @override
  String get providerPhotoUploadFailed =>
      'Não conseguimos enviar a foto. Tente de novo.';

  @override
  String get providerPhotoDelete => 'Apagar foto';

  @override
  String get providerServices => 'Serviços e preços';

  @override
  String get providerServicesEmpty => 'Cadastre ao menos um serviço.';

  @override
  String get providerServiceAdd => 'Adicionar serviço';

  @override
  String get providerServiceTitle => 'Nome do serviço';

  @override
  String get providerServiceDescription => 'Descrição (opcional)';

  @override
  String get providerServicePrice => 'A partir de';

  @override
  String get providerServiceOnQuote => 'Sob orçamento';

  @override
  String get providerServiceUnit => 'Cobrado por';

  @override
  String get providerUnitPerService => 'serviço';

  @override
  String get providerUnitPerHour => 'hora';

  @override
  String get providerUnitPerDay => 'dia';

  @override
  String get providerUnitPerSquareMeter => 'm²';

  @override
  String get providerUnitPerUnit => 'unidade';

  @override
  String providerPriceFrom(String price, String unit) {
    return 'a partir de $price/$unit';
  }

  @override
  String get providerServiceRemove => 'Remover serviço';

  @override
  String get providerReviewMissing => 'Para publicar, falta:';

  @override
  String get providerReviewReady => 'Tudo pronto para publicar.';

  @override
  String get providerMissingAbout => 'o texto sobre você';

  @override
  String get providerMissingCategories => 'uma categoria';

  @override
  String get providerMissingServiceArea => 'a área de atendimento';

  @override
  String get providerMissingServices => 'um serviço';

  @override
  String get providerMissingContactPhone => 'o WhatsApp para contato';

  @override
  String get providerPublished => 'Vitrine publicada.';

  @override
  String get providerStatusDraft => 'Rascunho';

  @override
  String get providerStatusPublished => 'Publicada';

  @override
  String get providerStatusPaused => 'Pausada';

  @override
  String get providerStatusSuspended => 'Suspensa';

  @override
  String get providerAvailableToday => 'Disponível hoje';

  @override
  String get providerAvailableTodayHint =>
      'Aparece no seu perfil até a meia-noite.';

  @override
  String providerCompleteness(int percent) {
    return 'Perfil $percent% completo';
  }

  @override
  String get providerNextStep => 'Próximo passo';

  @override
  String get providerNextPublish => 'Publicar a vitrine';

  @override
  String get providerNextShare => 'Compartilhar o perfil';

  @override
  String get providerNextAvailability => 'Informar os dias e horários';

  @override
  String get providerViewPublic => 'Ver como os clientes veem';

  @override
  String get providerShare => 'Compartilhar perfil';

  @override
  String providerShareText(String url) {
    return 'Veja meu perfil no Vitrio: $url';
  }

  @override
  String get providerAvailability => 'Dias e horários';

  @override
  String get providerEmergencies => 'Atende emergências';

  @override
  String get providerPause => 'Pausar a vitrine';

  @override
  String get providerResume => 'Publicar de novo';

  @override
  String get providerPausedHint =>
      'Vitrine pausada. Ela não aparece para os clientes.';

  @override
  String get providerDayMonday => 'Segunda';

  @override
  String get providerDayTuesday => 'Terça';

  @override
  String get providerDayWednesday => 'Quarta';

  @override
  String get providerDayThursday => 'Quinta';

  @override
  String get providerDayFriday => 'Sexta';

  @override
  String get providerDaySaturday => 'Sábado';

  @override
  String get providerDaySunday => 'Domingo';

  @override
  String get providerFrom => 'Das';

  @override
  String get providerTo => 'às';

  @override
  String get providerBadgeMei => 'MEI verificado';

  @override
  String get providerBadgeToday => 'Atende hoje';

  @override
  String get providerNew => 'Novo no Vitrio';

  @override
  String providerRating(String score, int count) {
    return 'Nota $score ($count avaliações)';
  }

  @override
  String providerServes(String city, int km) {
    return '$city · atende até $km km';
  }

  @override
  String providerYearsExperience(int years) {
    return '$years anos de experiência';
  }

  @override
  String get providerWhatsAppAction => 'WhatsApp';

  @override
  String get providerNotFound => 'Este perfil não está disponível.';

  @override
  String get providerAboutTitle => 'Sobre';

  @override
  String get providerWorks => 'Trabalhos';

  @override
  String get providerHours => 'Horários';

  @override
  String get authShow => 'Mostrar';

  @override
  String get authHide => 'Esconder';

  @override
  String get providerOfferServicesHint =>
      'Monte sua vitrine e receba pedidos de clientes da sua cidade.';

  @override
  String get welcomeHaveAccount => 'Já tem conta?';

  @override
  String get welcomeNearYou => 'Perto\nde você';

  @override
  String get welcomeSampleAnaName => 'Ana Paula';

  @override
  String get welcomeSampleAnaRole => 'Diarista';

  @override
  String get welcomeSampleAnaRating => '5,0';

  @override
  String get welcomeSampleJoseName => 'José Lima';

  @override
  String get welcomeSampleJoseRole => 'Pedreiro';

  @override
  String get welcomeSampleJoseRating => '4,8';

  @override
  String get welcomeSampleCarlosName => 'Carlos Menezes';

  @override
  String get welcomeSampleCarlosRole => 'Eletricista · Vila Serra';

  @override
  String get welcomeSampleCarlosRating => '4,9';

  @override
  String get welcomeSampleCarlosMeta => '(127 avaliações) · 1,2 km';

  @override
  String get notificationsTitle => 'Notificações';

  @override
  String get notificationsEmpty => 'Você ainda não tem notificações.';

  @override
  String homeGreeting(String name) {
    return 'Olá, $name';
  }

  @override
  String get homeQuestion => 'Do que você precisa hoje?';

  @override
  String get searchFieldLabel => 'Buscar serviço';

  @override
  String get searchPlaceholder => 'Eletricista, diarista, pintor…';

  @override
  String get homeHeroTag => 'Disponíveis agora';

  @override
  String get homeHeroTitle => 'Imprevisto em casa?';

  @override
  String get homeHeroText =>
      'Veja quem atende hoje perto de você, com nota e preço.';

  @override
  String get homeHeroAction => 'Ver disponíveis';

  @override
  String get homeCategories => 'Categorias';

  @override
  String get homeSeeAll => 'Ver todas';

  @override
  String get homeMore => 'Mais';

  @override
  String get tabAgenda => 'Agenda';

  @override
  String get providerOrdersEmpty =>
      'Você ainda não recebeu pedidos. Quando um cliente pedir orçamento, ele aparece aqui.';

  @override
  String get providerAgendaEmpty =>
      'Nenhum serviço agendado. Quando um cliente aceitar seu orçamento, o serviço aparece aqui.';

  @override
  String get providerNewOrders => 'Novos pedidos';

  @override
  String get providerNoOrders => 'Nenhum pedido novo por enquanto.';

  @override
  String get providerNoOrdersShare =>
      'Compartilhe seu perfil para chegar aos primeiros clientes.';

  @override
  String get providerNoOrdersPublish =>
      'Publique sua vitrine para começar a receber pedidos.';

  @override
  String get providerAvailableTitle => 'Disponível para novos pedidos';

  @override
  String get providerAvailableSubtitle => 'Você aparece em “Atende hoje”';

  @override
  String get providerUnavailableTitle => 'Pausado para novos pedidos';

  @override
  String get providerUnavailableSubtitle => 'Clientes ainda veem seu perfil';

  @override
  String get providerAvailableNeedsPublish =>
      'Fica disponível depois que a vitrine for publicada.';

  @override
  String get providerMetricNewOrders => 'pedidos novos';

  @override
  String get providerMetricRating => 'sua nota';

  @override
  String get providerMetricServicesMonth => 'serviços no mês';

  @override
  String get providerProfileAlmostReady => 'Seu perfil está quase pronto';

  @override
  String get providerProfileComplete => 'Seu perfil está completo';

  @override
  String providerPercent(int percent) {
    return '$percent%';
  }

  @override
  String get providerCompleteProfile => 'Completar perfil';

  @override
  String get providerHintAbout =>
      'Conte sobre você e o seu trabalho para os clientes te conhecerem.';

  @override
  String get providerHintCategories => 'Escolha as categorias do que você faz.';

  @override
  String get providerHintServiceArea => 'Informe onde você atende.';

  @override
  String get providerHintContactPhone => 'Informe o WhatsApp para contato.';

  @override
  String get providerHintServices => 'Cadastre seus serviços e preços.';

  @override
  String get providerHintPublish =>
      'Falta publicar a vitrine para aparecer nas buscas.';

  @override
  String get providerHintAvailability =>
      'Informe seus dias e horários para os clientes saberem quando você atende.';

  @override
  String get providerHintShare =>
      'Compartilhe seu perfil para receber os primeiros pedidos.';

  @override
  String get providerShareShort => 'Compartilhar';

  @override
  String get providerYourShowcase => 'Sua vitrine';

  @override
  String get providerSkip => 'Pular';

  @override
  String get providerAboutSubtitle =>
      'Seu nome, sua profissão e um pouco da sua história.';

  @override
  String get providerShowcaseSubtitle =>
      'Escolha o que você faz e onde atende. Dá para mudar quando quiser.';

  @override
  String get providerPhotosSubtitle =>
      'Mostre trabalhos que você já fez e quanto cobra pelos serviços.';

  @override
  String get providerReviewSubtitle =>
      'Confira como os clientes vão ver sua vitrine antes de publicar.';

  @override
  String get providerNextAfterAbout => 'Próximo: o que você faz e onde atende';

  @override
  String get providerNextAfterShowcase =>
      'Próximo: fotos dos seus trabalhos e preços';

  @override
  String get providerNextAfterPhotos => 'Próximo: revisar e publicar';

  @override
  String get providerCepPlaceholder => '00000-000';

  @override
  String get providerRadiusLabel => 'Raio de atendimento';

  @override
  String get providerContactPhoneHint =>
      'Usado no botão de WhatsApp do seu perfil.';

  @override
  String get providerPhonePlaceholder => '(16) 9 0000-0000';

  @override
  String get providerCnpjPlaceholder => '00.000.000/0001-00';

  @override
  String get providerServiceEdit => 'Editar serviço';

  @override
  String get providerServiceOnQuoteHint =>
      'O preço é combinado depois de ver o serviço.';

  @override
  String get providerFromLabel => 'a partir de';

  @override
  String get providerFromCapital => 'A partir de';

  @override
  String get providerPriceLabel => 'Preço';

  @override
  String get providerSeeProfile => 'Ver perfil';

  @override
  String get providerPreviewTitle => 'Como você aparece na busca';

  @override
  String providerMetricReviews(int count) {
    return '$count avaliações';
  }

  @override
  String providerDistance(String km) {
    return '$km km';
  }

  @override
  String get providerMetricDistance => 'de você';

  @override
  String get providerWhereWhen => 'Onde e quando atende';

  @override
  String providerRadiusFrom(int km, String place) {
    return 'Até $km km de $place';
  }

  @override
  String providerMoreNeighborhoods(int count) {
    return '+ $count bairros';
  }

  @override
  String get providerNoPhotos => 'Ainda não há fotos de trabalhos.';

  @override
  String get providerReviews => 'Avaliações';

  @override
  String get providerNoReviews =>
      'Ainda não há avaliações. Elas aparecem aqui depois dos primeiros serviços contratados pelo app.';

  @override
  String providerRatingOf(String score) {
    return 'Nota $score de 5';
  }

  @override
  String get providerDayShortMonday => 'seg';

  @override
  String get providerDayShortTuesday => 'ter';

  @override
  String get providerDayShortWednesday => 'qua';

  @override
  String get providerDayShortThursday => 'qui';

  @override
  String get providerDayShortFriday => 'sex';

  @override
  String get providerDayShortSaturday => 'sáb';

  @override
  String get providerDayShortSunday => 'dom';

  @override
  String providerDaysRange(String first, String last) {
    return '$first a $last';
  }

  @override
  String providerHoursRange(String start, String end) {
    return '$start às $end';
  }

  @override
  String get providerEmergenciesShort => 'atende emergências';

  @override
  String get providerCepNotFound =>
      'Não encontramos este CEP. Confira os 8 números do endereço base.';

  @override
  String get searchActivePlaceholder => 'O que você procura?';

  @override
  String get searchClear => 'Limpar busca';

  @override
  String get searchFilters => 'Filtros';

  @override
  String get searchNearMe => 'Perto de mim';

  @override
  String get searchTopRated => 'Nota 4,5+';

  @override
  String get searchLowestPrice => 'Menor preço';

  @override
  String get searchProfessionalOne => 'profissional';

  @override
  String get searchProfessionalMany => 'profissionais';

  @override
  String searchCountNearMe(String count, String professionals) {
    return '$count $professionals perto de você';
  }

  @override
  String searchCountInCity(String count, String professionals, String city) {
    return '$count $professionals em $city';
  }

  @override
  String searchMoreThanNearMe(String count, String professionals) {
    return 'Mais de $count $professionals perto de você';
  }

  @override
  String searchMoreThanInCity(String count, String professionals, String city) {
    return 'Mais de $count $professionals em $city';
  }

  @override
  String get searchSortTitle => 'Ordenar por';

  @override
  String get searchSortRelevance => 'Relevância';

  @override
  String get searchSortDistance => 'Mais perto';

  @override
  String get searchSortRating => 'Melhor avaliado';

  @override
  String get searchSortPrice => 'Menor preço';

  @override
  String get searchPriceFrom => 'A partir de';

  @override
  String get searchSeeProfile => 'Ver perfil';

  @override
  String searchReviewCount(String count) {
    return '($count)';
  }

  @override
  String get searchNoResults =>
      'Nenhum profissional encontrado com esses filtros.';

  @override
  String get searchClearFilters => 'Limpar filtros';

  @override
  String get searchError =>
      'Não conseguimos carregar os profissionais. Verifique a conexão e tente de novo.';

  @override
  String get searchChooseCity =>
      'Escolha uma cidade para ver os profissionais.';

  @override
  String get searchFilterDistance => 'Distância máxima';

  @override
  String searchFilterDistanceValue(String km) {
    return 'Até $km km';
  }

  @override
  String get searchFilterRating => 'Nota mínima';

  @override
  String get searchFilterAny => 'Qualquer';

  @override
  String searchFilterRatingValue(String rating) {
    return '$rating+';
  }

  @override
  String get searchFilterMeiHint => 'CNPJ ativo e optante pelo MEI';

  @override
  String get searchFilterTodayHint => 'Quem ligou “Disponível hoje”';

  @override
  String get searchFilterPrice => 'Preço a partir de';

  @override
  String searchFilterPriceValue(String price) {
    return 'Até $price';
  }

  @override
  String get searchFilterCity => 'Cidade';

  @override
  String get searchFilterApply => 'Ver resultados';

  @override
  String get searchFilterReset => 'Limpar';

  @override
  String get homeTopRated => 'Bem avaliados perto de você';

  @override
  String get homeSeeMore => 'Ver mais';

  @override
  String emptyCityTitle(String city) {
    return 'Ainda não tem profissionais do Vitrio em $city.';
  }

  @override
  String get emptyCityTitleNearby =>
      'Ainda não tem profissionais do Vitrio perto de você.';

  @override
  String get emptyCityText =>
      'Conhece alguém que faz um bom trabalho? Mande o Vitrio para essa pessoa.';

  @override
  String get emptyCityInvite => 'Indicar o Vitrio';

  @override
  String emptyCityShareText(String url) {
    return 'Conheça o Vitrio, o app para achar profissionais de confiança perto de você: $url';
  }

  @override
  String get favoritesTitle => 'Favoritos';

  @override
  String get favoritesEmpty => 'Você ainda não salvou nenhum profissional.';

  @override
  String get favoritesSearch => 'Buscar profissionais';

  @override
  String get favoritesError =>
      'Não conseguimos carregar seus favoritos. Verifique a conexão e tente de novo.';

  @override
  String get favoriteSave => 'Salvar nos favoritos';

  @override
  String get favoriteRemove => 'Remover dos favoritos';

  @override
  String get favoriteFailed =>
      'Não conseguimos atualizar seus favoritos. Tente de novo.';
}
