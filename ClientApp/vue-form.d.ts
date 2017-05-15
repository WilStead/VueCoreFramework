export interface FormState {
    $dirty?: boolean,
    $pristine?: boolean,
    $valid?: boolean,
    $invalid?: boolean,
    $submitted?: boolean,
    $touched?: boolean,
    $untouched?: boolean,
    $pending?: boolean,
    $error?: any,
    $submittedState?: any
}