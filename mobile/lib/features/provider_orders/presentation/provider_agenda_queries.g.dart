// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'provider_agenda_queries.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(providerAgenda)
final providerAgendaProvider = ProviderAgendaFamily._();

final class ProviderAgendaProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<AgendaItem>>,
          List<AgendaItem>,
          FutureOr<List<AgendaItem>>
        >
    with $FutureModifier<List<AgendaItem>>, $FutureProvider<List<AgendaItem>> {
  ProviderAgendaProvider._({
    required ProviderAgendaFamily super.from,
    required (DateTime, DateTime) super.argument,
  }) : super(
         retry: noRetry,
         name: r'providerAgendaProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$providerAgendaHash();

  @override
  String toString() {
    return r'providerAgendaProvider'
        ''
        '$argument';
  }

  @$internal
  @override
  $FutureProviderElement<List<AgendaItem>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<AgendaItem>> create(Ref ref) {
    final argument = this.argument as (DateTime, DateTime);
    return providerAgenda(ref, argument.$1, argument.$2);
  }

  @override
  bool operator ==(Object other) {
    return other is ProviderAgendaProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$providerAgendaHash() => r'edd0fd44ee4ed495893845f39015e6ee38a0e574';

final class ProviderAgendaFamily extends $Family
    with
        $FunctionalFamilyOverride<
          FutureOr<List<AgendaItem>>,
          (DateTime, DateTime)
        > {
  ProviderAgendaFamily._()
    : super(
        retry: noRetry,
        name: r'providerAgendaProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  ProviderAgendaProvider call(DateTime from, DateTime to) =>
      ProviderAgendaProvider._(argument: (from, to), from: this);

  @override
  String toString() => r'providerAgendaProvider';
}
