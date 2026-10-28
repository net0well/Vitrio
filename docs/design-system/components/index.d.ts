import type * as React from 'react';

export type IconName =
  | 'search' | 'pin' | 'star' | 'home' | 'orders' | 'user' | 'chart'
  | 'chevron-left' | 'chevron-right' | 'chevron-down' | 'arrow-right' | 'sliders'
  | 'check' | 'x' | 'plus' | 'shield-check' | 'bell' | 'heart' | 'share' | 'message' | 'send'
  | 'camera' | 'image' | 'calendar' | 'clock' | 'info' | 'lock' | 'flag'
  | 'wallet' | 'qr' | 'cash' | 'card' | 'transfer'
  | 'bolt' | 'droplet' | 'brick' | 'roller' | 'broom' | 'leaf' | 'wrench' | 'grid';

export type Tone = 'plum' | 'green' | 'amber' | 'info' | 'lime';
export type BadgeTone = 'verified' | 'brand' | 'neutral' | 'warning' | 'accent' | 'danger' | 'info';
export type PaymentMethod = 'pix' | 'dinheiro' | 'debito' | 'credito' | 'transferencia' | 'outro';

export interface IconProps { name: IconName; size?: number; strokeWidth?: number; filled?: boolean; label?: string; className?: string; style?: React.CSSProperties }
export declare function Icon(props: IconProps): React.ReactElement | null;

export interface ButtonProps {
  variant?: 'primary' | 'accent' | 'secondary' | 'ghost' | 'on-hero' | 'danger';
  size?: 'md' | 'lg';
  block?: boolean;
  icon?: IconName;
  trailingIcon?: IconName;
  href?: string;
  type?: 'button' | 'submit';
  disabled?: boolean;
  onClick?: () => void;
  className?: string;
  children?: React.ReactNode;
}
export declare function Button(props: ButtonProps): React.ReactElement;

export interface IconButtonProps { icon: IconName; label: string; variant?: 'surface' | 'soft' | 'solid'; dot?: boolean; href?: string; onClick?: () => void; iconSize?: number; className?: string }
export declare function IconButton(props: IconButtonProps): React.ReactElement;

export interface SearchFieldProps { label?: string; placeholder?: string; defaultValue?: string; onSubmit?: (query: string) => void; className?: string }
export declare function SearchField(props: SearchFieldProps): React.ReactElement;

export interface TextFieldProps {
  label: string;
  value?: string;
  defaultValue?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  prefix?: string;
  icon?: IconName;
  hint?: string;
  error?: string;
  maxLength?: number;
  type?: string;
  inputMode?: 'text' | 'numeric' | 'decimal' | 'tel' | 'email' | 'search';
  className?: string;
}
export declare function TextField(props: TextFieldProps): React.ReactElement;

export interface TextAreaProps { label: string; value?: string; defaultValue?: string; onChange?: (value: string) => void; placeholder?: string; hint?: string; error?: string; maxLength?: number; rows?: number; className?: string }
export declare function TextArea(props: TextAreaProps): React.ReactElement;

export interface ChipProps { selected?: boolean; defaultSelected?: boolean; onChange?: (selected: boolean) => void; variant?: 'solid' | 'soft'; className?: string; children?: React.ReactNode }
export declare function Chip(props: ChipProps): React.ReactElement;

export interface SegmentedOption { value: string; label: string }
export interface SegmentedControlProps { label: string; options: SegmentedOption[]; value?: string; defaultValue?: string; onChange?: (value: string) => void; className?: string }
export declare function SegmentedControl(props: SegmentedControlProps): React.ReactElement;

export interface SwitchProps { label: string; description?: string; checked?: boolean; defaultChecked?: boolean; onChange?: (checked: boolean) => void; className?: string }
export declare function Switch(props: SwitchProps): React.ReactElement;

export interface RatingInputProps { value?: number; defaultValue?: number; onChange?: (value: number) => void; label?: string; className?: string }
export declare function RatingInput(props: RatingInputProps): React.ReactElement;

export interface PaymentMethodOption { value: PaymentMethod; label: string; icon: IconName }
export interface PaymentMethodPickerProps { value?: PaymentMethod; defaultValue?: PaymentMethod; onChange?: (value: PaymentMethod) => void; label?: string; methods?: PaymentMethodOption[]; className?: string }
export declare function PaymentMethodPicker(props: PaymentMethodPickerProps): React.ReactElement;

export interface BadgeProps { tone?: BadgeTone; icon?: IconName; dot?: boolean; className?: string; children?: React.ReactNode }
export declare function Badge(props: BadgeProps): React.ReactElement;

export interface AvatarProps { initials: string; name: string; tone?: Tone; size?: number; kind?: 'provider' | 'client'; className?: string }
export declare function Avatar(props: AvatarProps): React.ReactElement;

export interface RatingProps { value: number; count?: number; meta?: string; variant?: 'compact' | 'stars'; size?: number; className?: string }
export declare function Rating(props: RatingProps): React.ReactElement;

export interface CategoryTileProps { icon: IconName; label: string; href?: string; more?: boolean; className?: string }
export declare function CategoryTile(props: CategoryTileProps): React.ReactElement;

export interface HeroCardProps { title: string; text?: string; tag?: string; actionLabel?: string; href?: string; icon?: IconName; className?: string }
export declare function HeroCard(props: HeroCardProps): React.ReactElement;

export interface ProviderBadge { tone: BadgeTone; label: string; icon?: IconName }
export interface ProviderCardProps {
  name: string;
  initials: string;
  tone?: Tone;
  profession: string;
  rating: number;
  reviews: number;
  distance?: string;
  neighborhood: string;
  verified?: boolean;
  badges?: ProviderBadge[];
  priceFrom?: number | string;
  href?: string;
  className?: string;
}
export declare function ProviderCard(props: ProviderCardProps): React.ReactElement;

export interface ReviewReply { author: string; text: string }
export interface ReviewCardProps { author: string; initials: string; tone?: Tone; date: string; service?: string; rating: number; text?: string; tags?: string[]; reply?: ReviewReply; className?: string }
export declare function ReviewCard(props: ReviewCardProps): React.ReactElement;

export interface TimelineStep { title: string; sub?: string; state: 'done' | 'current' | 'pending' }
export interface StatusTimelineProps { steps: TimelineStep[]; label?: string; className?: string }
export declare function StatusTimeline(props: StatusTimelineProps): React.ReactElement;

export interface QuoteRow { icon: IconName; text: string }
export interface QuoteCardProps { from: string; price: number | string; validity?: string; rows?: QuoteRow[]; message?: string; onAccept?: () => void; onDecline?: () => void; acceptLabel?: string; declineLabel?: string; className?: string }
export declare function QuoteCard(props: QuoteCardProps): React.ReactElement;

export interface PaymentReceiptProps { amount: number | string; method: PaymentMethod; date: string; confirmedBy: string; note?: string; onDispute?: () => void; className?: string }
export declare function PaymentReceipt(props: PaymentReceiptProps): React.ReactElement;

export interface NavItem { icon: IconName; label: string; href?: string; current?: boolean }
export interface BottomNavProps { items: NavItem[]; label?: string; className?: string }
export declare function BottomNav(props: BottomNavProps): React.ReactElement;

export interface TopBarProps { title: string; backHref?: string; onBack?: () => void; backIcon?: IconName; backLabel?: string; trailing?: React.ReactNode; className?: string }
export declare function TopBar(props: TopBarProps): React.ReactElement;

export interface ActionBarProps { note?: string; className?: string; children?: React.ReactNode }
export declare function ActionBar(props: ActionBarProps): React.ReactElement;

declare global {
  interface Window {
    Vitrio: {
      Icon: typeof Icon; Button: typeof Button; IconButton: typeof IconButton; SearchField: typeof SearchField;
      TextField: typeof TextField; TextArea: typeof TextArea; Chip: typeof Chip; SegmentedControl: typeof SegmentedControl;
      Switch: typeof Switch; RatingInput: typeof RatingInput; PaymentMethodPicker: typeof PaymentMethodPicker; Badge: typeof Badge;
      Avatar: typeof Avatar; Rating: typeof Rating; CategoryTile: typeof CategoryTile; HeroCard: typeof HeroCard;
      ProviderCard: typeof ProviderCard; ReviewCard: typeof ReviewCard; StatusTimeline: typeof StatusTimeline; QuoteCard: typeof QuoteCard;
      PaymentReceipt: typeof PaymentReceipt; BottomNav: typeof BottomNav; TopBar: typeof TopBar; ActionBar: typeof ActionBar;
      ICON_NAMES: IconName[]; PAYMENT_METHODS: PaymentMethodOption[];
    };
  }
}
